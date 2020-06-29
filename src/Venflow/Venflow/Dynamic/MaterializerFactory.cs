using Npgsql.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;
using Venflow.Enums;
using Venflow.Commands;

namespace Venflow.Dynamic
{
    internal class MaterializerFactory<TEntity> where TEntity : class
    {
        private readonly Entity<TEntity> _entity;
        private readonly Dictionary<int, Func<NpgsqlDataReader, Task<List<TEntity>>>> _materializerCache;
        private readonly object _materializerLock;

        internal MaterializerFactory(Entity<TEntity> entity)
        {
            _entity = entity;

            _materializerCache = new Dictionary<int, Func<NpgsqlDataReader, Task<List<TEntity>>>>();
            _materializerLock = new object();
        }

        internal Func<NpgsqlDataReader, Task<List<TEntity>>> GetOrCreateMaterializer(JoinBuilderValues? joinBuilderValues, DbConfiguration dbConfiguration, ReadOnlyCollection<NpgsqlDbColumn> columnSchema)
        {
            var cacheKeyBuilder = new HashCode();

            cacheKeyBuilder.Add(_entity.TableName);

            for (int i = 0; i < columnSchema.Count; i++)
            {
                var column = columnSchema[i];

                if (TryGetEntityOfTable(dbConfiguration, column, out var entity, out var columnName))
                {

                    cacheKeyBuilder.Add(entity.TableName);
                    cacheKeyBuilder.Add(columnName);
                }
                else
                {
                    cacheKeyBuilder.Add(column.ColumnName);
                }
            }

            var cacheKey = cacheKeyBuilder.ToHashCode();

            lock (_materializerLock)
            {
                if (_materializerCache.TryGetValue(cacheKey, out var materializer))
                {
                    return materializer;
                }
                else
                {
                    var entities = new List<KeyValuePair<Entity, List<KeyValuePair<string, int>>>>();
                    var columns = new List<KeyValuePair<string, int>>();

                    entities.Add(new KeyValuePair<Entity, List<KeyValuePair<string, int>>>(_entity, columns));

                    for (int i = 0; i < columnSchema.Count; i++)
                    {
                        var column = columnSchema[i];

                        if (TryGetEntityOfTable(dbConfiguration, column, out var entity, out var columnName))
                        {
                            columns = new List<KeyValuePair<string, int>>()
                            {
                                new KeyValuePair<string, int>(columnName, column.ColumnOrdinal.Value)
                            };

                            entities.Add(new KeyValuePair<Entity, List<KeyValuePair<string, int>>>(entity, columns));
                        }
                        else
                        {
                            columns.Add(new KeyValuePair<string, int>(column.ColumnName, column.ColumnOrdinal.Value));
                        }
                    }

                    if (joinBuilderValues is { })
                    {
                        if (joinBuilderValues.Joins.Count + 1 > entities.Count)
                        {
                            throw new InvalidOperationException("You configured more joins than entities returned by the query.");
                        }
                        else if (joinBuilderValues.Joins.Count + 1 < entities.Count)
                        {
                            throw new InvalidOperationException("You configured fewer joins than entities returned by the query.");
                        }
                    }
                    else if (entities.Count > 0)
                    {
                        throw new InvalidOperationException("The result set contained multiple tables, however the query was configured to only expect one. Try specifying the tables you are joining with JoinWith, while declaring the query.");
                    }

                    materializer = CreateMaterializer(joinBuilderValues, entities);

                    _materializerCache.TryAdd(cacheKey, materializer);

                    return materializer;
                }
            }
        }

        private Func<NpgsqlDataReader, Task<List<TEntity>>> CreateMaterializer(JoinBuilderValues? joinBuilderValues, List<KeyValuePair<Entity, List<KeyValuePair<string, int>>>> entities)
        {
            var primaryEntity = entities[0];
            var genericListType = typeof(List<>);
            var primaryEntityListType = genericListType.MakeGenericType(primaryEntity.Key.EntityType);

            var asyncStateMachineType = typeof(IAsyncStateMachine);
            var asyncMethodBuilderType = typeof(AsyncTaskMethodBuilder<>).MakeGenericType(primaryEntityListType);
            var npgsqlDataReaderType = typeof(NpgsqlDataReader);
            var taskAwaiterType = typeof(TaskAwaiter<bool>);
            var intType = typeof(int);
            var taskBoolType = typeof(Task<bool>);
            var exceptionType = typeof(Exception);

            var materializerTypeBuilder = TypeFactory.GetNewMaterializerBuilder(_entity.EntityName,
                TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Abstract |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            var stateMachineTypeBuilder = materializerTypeBuilder.DefineNestedType("StateMachine",
                TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, typeof(ValueType),
                new[] { asyncStateMachineType });

            var stateField = stateMachineTypeBuilder.DefineField("_state", intType, FieldAttributes.Public);
            var methodBuilderField = stateMachineTypeBuilder.DefineField("_builder", asyncMethodBuilderType, FieldAttributes.Public);
            var dataReaderField = stateMachineTypeBuilder.DefineField("_dataReader", npgsqlDataReaderType, FieldAttributes.Public);
            var taskAwaiterField = stateMachineTypeBuilder.DefineField("_awaiter", taskAwaiterType, FieldAttributes.Private);

            var moveNextMethod = stateMachineTypeBuilder.DefineMethod("MoveNext",
                MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual);
            var moveNextMethodIL = moveNextMethod.GetILGenerator();

            var stateLocal = moveNextMethodIL.DeclareLocal(intType);
            var awaiterLocal = moveNextMethodIL.DeclareLocal(taskAwaiterType);
            var exceptionLocal = moveNextMethodIL.DeclareLocal(exceptionType);
            var primaryListLocal = moveNextMethodIL.DeclareLocal(primaryEntityListType);

            var endOfMethodLabel = moveNextMethodIL.DefineLabel();

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, stateField);
            moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);

            var exceptionBlock = moveNextMethodIL.BeginExceptionBlock();
            var ifGotoLabel = moveNextMethodIL.DefineLabel();

            moveNextMethodIL.Emit(OpCodes.Ldloc, stateLocal);
            moveNextMethodIL.Emit(OpCodes.Brfalse, ifGotoLabel);

            // -- Actual Local and Field Assignment

            var primaryEntityListField = stateMachineTypeBuilder.DefineField(primaryEntity.Key.EntityName + "List", primaryEntityListType, FieldAttributes.Private);

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Newobj, primaryEntityListType.GetConstructor(Type.EmptyTypes));
            moveNextMethodIL.Emit(OpCodes.Stfld, primaryEntityListField);

            var iLSetNullGhostGen = new ILGhostGenerator();

            iLSetNullGhostGen.Emit(OpCodes.Ldarg_0);
            iLSetNullGhostGen.Emit(OpCodes.Ldnull);
            iLSetNullGhostGen.Emit(OpCodes.Stfld, primaryEntityListField);

            var iLGhostBodyGen = new ILGhostGenerator();

            var afterBodyLabel = moveNextMethodIL.DefineLabel();
            var beforeBodyLabel = moveNextMethodIL.DefineLabel();

            if (entities.Count > 1)
            {
                var gernericDictionaryType = typeof(Dictionary<,>);

                Label? nextIfStartLabel = default;

                var lastEntityFieldDictionary = new Dictionary<string, FieldInfo>();
                var relationAssignments = new List<EntityRelationAssignment>();

                for (int i = 0; i < entities.Count; i++)
                {
                    var entityWithColumns = entities[i];
                    var entity = entityWithColumns.Key;
                    var columns = entityWithColumns.Value;

                    var primaryKeyType = entity.GetPrimaryColumn().PropertyInfo.PropertyType;
                    var entityDictionaryType = gernericDictionaryType.MakeGenericType(primaryKeyType, entity.EntityType);

                    var lastEntityField = stateMachineTypeBuilder.DefineField("last" + entity.EntityName + i, entity.EntityType, FieldAttributes.Private);
                    var entityDictionaryField = stateMachineTypeBuilder.DefineField(entity.EntityName + "Dict" + i, entityDictionaryType, FieldAttributes.Private);

                    var shouldCheckForChange = false;

                    if (entity.Relations is { } && entity.Relations.Count > 0)
                    {
                        for (int k = 0; k < joinBuilderValues.UsedRelations.Count; k++)
                        {
                            var expectedJoinId = joinBuilderValues.UsedRelations[k];

                            for (int z = 0; z < entity.Relations.Count; z++)
                            {
                                var relation = entity.Relations[z];

                                if (relation.RelationId == expectedJoinId &&
                                    relation.RelationType != RelationType.OneToMany)
                                {
                                    shouldCheckForChange = true;

                                    break;
                                }
                            }

                            if (shouldCheckForChange)
                            {
                                break;
                            }
                        }
                    }

                    // TODO: Try to find a way to avoid useless field.

                    FieldBuilder? hasEntityChangedField = null;

                    if (shouldCheckForChange)
                    {
                        hasEntityChangedField = stateMachineTypeBuilder.DefineField("has" + entity.EntityName + "Changed" + i, entityDictionaryType, FieldAttributes.Private);

                        moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                        moveNextMethodIL.Emit(OpCodes.Stfld, hasEntityChangedField);
                    }

                    var primaryKeyLocal = moveNextMethodIL.DeclareLocal(primaryKeyType);
                    var entityLocal = moveNextMethodIL.DeclareLocal(entity.EntityType);

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldnull);
                    moveNextMethodIL.Emit(OpCodes.Stfld, lastEntityField);

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Newobj, entityDictionaryType.GetConstructor(Type.EmptyTypes));
                    moveNextMethodIL.Emit(OpCodes.Stfld, entityDictionaryField);

                    var primaryKeyColumnScheme = columns[0];
                    var primaryKeyColumn = entity.GetColumn(primaryKeyColumnScheme.Key);

                    if (nextIfStartLabel.HasValue)
                    {
                        iLGhostBodyGen.MarkLabel(nextIfStartLabel.Value);
                    }

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4, primaryKeyColumnScheme.Value);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryKeyColumn.DbValueRetriever);
                    iLGhostBodyGen.Emit(OpCodes.Stloc, primaryKeyLocal);

                    var lastEntityIfBody = moveNextMethodIL.DefineLabel();

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, lastEntityField);
                    iLGhostBodyGen.Emit(OpCodes.Brfalse, lastEntityIfBody);

                    nextIfStartLabel = moveNextMethodIL.DefineLabel();

                    iLGhostBodyGen.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, lastEntityField);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryKeyColumn.PropertyInfo.GetGetMethod());
                    iLGhostBodyGen.Emit(OpCodes.Beq, nextIfStartLabel.Value);

                    iLGhostBodyGen.MarkLabel(lastEntityIfBody);

                    var entityDictionaryIfBodyEnd = moveNextMethodIL.DefineLabel();

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, entityDictionaryField);
                    iLGhostBodyGen.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    iLGhostBodyGen.Emit(OpCodes.Ldloca, entityLocal);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, entityDictionaryType.GetMethod("TryGetValue"));
                    iLGhostBodyGen.Emit(OpCodes.Brfalse, entityDictionaryIfBodyEnd);

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldloca, entityLocal);
                    iLGhostBodyGen.Emit(OpCodes.Stfld, lastEntityField);
                    iLGhostBodyGen.Emit(OpCodes.Br, nextIfStartLabel.Value);

                    iLGhostBodyGen.MarkLabel(entityDictionaryIfBodyEnd);

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Newobj, entity.EntityType.GetConstructor(Type.EmptyTypes));

                    iLGhostBodyGen.Emit(OpCodes.Dup);
                    iLGhostBodyGen.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryKeyColumn.PropertyInfo.GetSetMethod());

                    for (int k = 1; k < columns.Count; k++)
                    {
                        var columnScheme = columns[k];

                        if (!entity.TryGetColumn(columnScheme.Key, out var column))
                        {
                            throw new InvalidOperationException($"There is no column mapped to column '{columnScheme.Key}' on Entity '{entity.EntityName}'");
                        }

                        iLGhostBodyGen.Emit(OpCodes.Dup);
                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                        iLGhostBodyGen.Emit(OpCodes.Ldc_I4, columnScheme.Value);
                        iLGhostBodyGen.Emit(OpCodes.Callvirt, column.DbValueRetriever);
                        iLGhostBodyGen.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
                    }

                    if (entity.Relations is { } && entity.Relations.Count > 0)
                    {
                        EntityRelationAssignment? entityRelationAssignment = null;

                        if (shouldCheckForChange)
                        {
                            entityRelationAssignment = new EntityRelationAssignment(lastEntityField, hasEntityChangedField);
                        }

                        for (int k = 0; k < entity.Relations.Count; k++)
                        {
                            var relation = entity.Relations[k];

                            lastEntityFieldDictionary.Add(entity.EntityName + relation.RightEntity.EntityName + (relation.LeftNavigationProperty ?? relation.RightNavigationProperty).Name, lastEntityField);

                            if (relation.RelationType != RelationType.OneToMany)
                            {
                                if (shouldCheckForChange)
                                {
                                    entityRelationAssignment.Relations.Add(new RelationAssignmentInformation(relation, relation.RightEntity.EntityName + entity.EntityName + (relation.RightNavigationProperty ?? relation.LeftNavigationProperty).Name));
                                }

                                continue;
                            }

                            iLGhostBodyGen.Emit(OpCodes.Dup);

                            iLGhostBodyGen.Emit(OpCodes.Newobj, relation.LeftNavigationProperty.PropertyType.GetConstructor(Type.EmptyTypes));
                            iLGhostBodyGen.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetSetMethod());
                        }

                        relationAssignments.Add(entityRelationAssignment);
                    }

                    iLGhostBodyGen.Emit(OpCodes.Stfld, lastEntityField);

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, entityDictionaryField);
                    iLGhostBodyGen.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, lastEntityField);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, entityDictionaryType.GetMethod("Add"));

                    if (i == 0)
                    {
                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldfld, primaryEntityListField);
                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldfld, lastEntityField);
                        iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryEntityListType.GetMethod("Add"));
                    }

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4_1);
                    iLGhostBodyGen.Emit(OpCodes.Stfld, hasEntityChangedField);

                    iLSetNullGhostGen.Emit(OpCodes.Ldarg_0);
                    iLSetNullGhostGen.Emit(OpCodes.Ldnull);
                    iLSetNullGhostGen.Emit(OpCodes.Stfld, lastEntityField);

                    iLSetNullGhostGen.Emit(OpCodes.Ldarg_0);
                    iLSetNullGhostGen.Emit(OpCodes.Ldnull);
                    iLSetNullGhostGen.Emit(OpCodes.Stfld, entityDictionaryField);
                }

                iLGhostBodyGen.MarkLabel(nextIfStartLabel.Value);

                Label? nextAssignmentIfStartLabel = default;

                for (int i = 0; i < relationAssignments.Count; i++)
                {
                    var entityRelationAssignment = relationAssignments[i];

                    for (int k = 0; k < entityRelationAssignment.Relations.Count; k++)
                    {
                        var relationAssignment = entityRelationAssignment.Relations[k];

                        if (!lastEntityFieldDictionary.TryGetValue(relationAssignment.LastRightName, out var lastRightEntityField))
                        {
                            continue;
                        }

                        if (i > 0)
                        {
                            iLGhostBodyGen.MarkLabel(nextAssignmentIfStartLabel.Value);
                        }

                        nextAssignmentIfStartLabel = moveNextMethodIL.DefineLabel();

                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldfld, entityRelationAssignment.HasLastLeftEntityChanged);
                        iLGhostBodyGen.Emit(OpCodes.Brfalse, i == relationAssignments.Count - 1 ? afterBodyLabel : nextAssignmentIfStartLabel.Value);

                        if (relationAssignment.EntityRelation.RelationType == RelationType.OneToOne)
                        {
                            if (relationAssignment.EntityRelation.LeftNavigationProperty is { } &&
                                relationAssignment.EntityRelation.LeftNavigationProperty.PropertyType == lastRightEntityField.FieldType)
                            {
                                iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                                iLGhostBodyGen.Emit(OpCodes.Ldfld, entityRelationAssignment.LastLeftEntity);
                                iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                                iLGhostBodyGen.Emit(OpCodes.Ldfld, lastRightEntityField);
                                iLGhostBodyGen.Emit(OpCodes.Callvirt, relationAssignment.EntityRelation.LeftNavigationProperty.GetSetMethod());
                            }
                        }
                        else if (relationAssignment.EntityRelation.RelationType == RelationType.ManyToOne)
                        {
                            iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                            iLGhostBodyGen.Emit(OpCodes.Ldfld, lastRightEntityField);
                            iLGhostBodyGen.Emit(OpCodes.Callvirt, relationAssignment.EntityRelation.RightNavigationProperty.GetGetMethod());
                            iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                            iLGhostBodyGen.Emit(OpCodes.Ldfld, entityRelationAssignment.LastLeftEntity);
                            iLGhostBodyGen.Emit(OpCodes.Callvirt, genericListType.MakeGenericType(relationAssignment.EntityRelation.LeftEntity.EntityType).GetMethod("Add"));

                            iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                            iLGhostBodyGen.Emit(OpCodes.Ldfld, entityRelationAssignment.LastLeftEntity);
                            iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                            iLGhostBodyGen.Emit(OpCodes.Ldfld, lastRightEntityField);
                            iLGhostBodyGen.Emit(OpCodes.Callvirt, relationAssignment.EntityRelation.LeftNavigationProperty.GetSetMethod());
                        }
                    }
                }
            }
            else
            {
                var columns = entities[0].Value;

                iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                iLGhostBodyGen.Emit(OpCodes.Ldfld, primaryEntityListField);
                iLGhostBodyGen.Emit(OpCodes.Newobj, _entity.EntityType.GetConstructor(Type.EmptyTypes));

                for (int k = 0; k < columns.Count; k++)
                {
                    var columnScheme = columns[k];
                    var column = _entity.GetColumn(columnScheme.Key);

                    iLGhostBodyGen.Emit(OpCodes.Dup);
                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4, columnScheme.Value);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, column.DbValueRetriever);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
                }

                iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryEntityListType.GetMethod("Add"));
            }


            // -- End of actual Local and Field Assignment

            moveNextMethodIL.Emit(OpCodes.Br, afterBodyLabel);

            moveNextMethodIL.MarkLabel(beforeBodyLabel);

            // -- Actual Method Body (in while ReadAsync loop)

            iLGhostBodyGen.WriteIL(moveNextMethodIL);

            // -- End of actual Method Body (in while ReadAsync loop)

            moveNextMethodIL.MarkLabel(afterBodyLabel);

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
            moveNextMethodIL.Emit(OpCodes.Callvirt,
                npgsqlDataReaderType.GetMethod("ReadAsync", Type.EmptyTypes));
            moveNextMethodIL.Emit(OpCodes.Callvirt,
                taskBoolType.GetMethod("GetAwaiter")); // GetAwaiter
            moveNextMethodIL.Emit(OpCodes.Stloc, awaiterLocal);

            // Check if TaskAwaiter isn't complete

            moveNextMethodIL.Emit(OpCodes.Ldloca, awaiterLocal);
            moveNextMethodIL.Emit(OpCodes.Call, taskAwaiterType.GetProperty("IsCompleted").GetGetMethod());

            var catchEndLabel = moveNextMethodIL.DefineLabel();
            moveNextMethodIL.Emit(OpCodes.Brtrue, catchEndLabel);

            // If TaskAwaiter isn't complete

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
            moveNextMethodIL.Emit(OpCodes.Dup);
            moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
            moveNextMethodIL.Emit(OpCodes.Stfld, stateField);
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldloc, awaiterLocal);
            moveNextMethodIL.Emit(OpCodes.Stfld, taskAwaiterField);
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            moveNextMethodIL.Emit(OpCodes.Ldloca, awaiterLocal);
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Call,
               asyncMethodBuilderType.GetMethod("AwaitUnsafeOnCompleted")
                    .MakeGenericMethod(taskAwaiterType, stateMachineTypeBuilder));

            moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            // If TaskAwaiter is complete

            moveNextMethodIL.MarkLabel(ifGotoLabel);
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, taskAwaiterField);
            moveNextMethodIL.Emit(OpCodes.Stloc, awaiterLocal);
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldflda, taskAwaiterField);
            moveNextMethodIL.Emit(OpCodes.Initobj, taskAwaiterType);
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
            moveNextMethodIL.Emit(OpCodes.Dup);
            moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
            moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

            // End of If

            // Call GetResult

            moveNextMethodIL.MarkLabel(catchEndLabel);
            moveNextMethodIL.Emit(OpCodes.Ldloca, awaiterLocal);
            moveNextMethodIL.Emit(OpCodes.Call, taskAwaiterType.GetMethod("GetResult"));
            moveNextMethodIL.Emit(OpCodes.Brtrue, beforeBodyLabel);

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, primaryEntityListField);
            moveNextMethodIL.Emit(OpCodes.Stloc, primaryListLocal);

            moveNextMethodIL.Emit(OpCodes.Leave, exceptionBlock);

            // Start of Catch Block

            moveNextMethodIL.BeginCatchBlock(exceptionType);

            moveNextMethodIL.Emit(OpCodes.Stloc, exceptionLocal);
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

            iLSetNullGhostGen.WriteIL(moveNextMethodIL);

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            moveNextMethodIL.Emit(OpCodes.Ldloc, exceptionLocal);
            moveNextMethodIL.Emit(OpCodes.Call, asyncMethodBuilderType.GetMethod("SetException"));
            moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            moveNextMethodIL.EndExceptionBlock();

            // End Of Catch

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

            iLSetNullGhostGen.WriteIL(moveNextMethodIL);

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            moveNextMethodIL.Emit(OpCodes.Ldloc, primaryListLocal);
            moveNextMethodIL.Emit(OpCodes.Call, asyncMethodBuilderType.GetMethod("SetResult"));

            // End of Method

            moveNextMethodIL.MarkLabel(endOfMethodLabel);
            moveNextMethodIL.Emit(OpCodes.Ret);

            stateMachineTypeBuilder.DefineMethodOverride(moveNextMethod,
                asyncStateMachineType.GetMethod("MoveNext"));

            var setStateMachineMethod = stateMachineTypeBuilder.DefineMethod("SetStateMachine",
                MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual, null, new[] { asyncStateMachineType });
            var setStateMachineMethodIL = setStateMachineMethod.GetILGenerator();

            setStateMachineMethodIL.Emit(OpCodes.Ldarg_0);
            setStateMachineMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            setStateMachineMethodIL.Emit(OpCodes.Ldarg_1);
            setStateMachineMethodIL.Emit(OpCodes.Call, asyncMethodBuilderType.GetMethod("SetStateMachine"));
            setStateMachineMethodIL.Emit(OpCodes.Ret);

            stateMachineTypeBuilder.DefineMethodOverride(setStateMachineMethod,
                asyncStateMachineType.GetMethod("SetStateMachine"));

            var materializeMethod = materializerTypeBuilder.DefineMethod("MaterializeAsync",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, typeof(Task<List<TEntity>>),
                new[] { npgsqlDataReaderType });

            materializeMethod.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(AsyncStateMachineAttribute).GetConstructor(new[] { typeof(Type) }),
                new[] { stateMachineTypeBuilder }));

            var materializeMethodIL = materializeMethod.GetILGenerator();

            materializeMethodIL.DeclareLocal(stateMachineTypeBuilder);

            // Create and execute the StateMachine

            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldarg_0);
            materializeMethodIL.Emit(OpCodes.Stfld, dataReaderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Call,
                asyncMethodBuilderType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static));
            materializeMethodIL.Emit(OpCodes.Stfld, methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldc_I4_M1);
            materializeMethodIL.Emit(OpCodes.Stfld, stateField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Call,
               asyncMethodBuilderType.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(stateMachineTypeBuilder));
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Call, asyncMethodBuilderType.GetProperty("Task").GetGetMethod());

            materializeMethodIL.Emit(OpCodes.Ret);

            stateMachineTypeBuilder.CreateType();
            var materializerType = materializerTypeBuilder.CreateType();

            return (Func<NpgsqlDataReader, Task<List<TEntity>>>)materializerType.GetMethod("MaterializeAsync").CreateDelegate(typeof(Func<NpgsqlDataReader, Task<List<TEntity>>>));
        }

        private bool TryGetEntityOfTable(DbConfiguration dbConfiguration, NpgsqlDbColumn column, out Entity? entity,
            out string? columnName)
        {
            entity = null;

            if (column.ColumnName[0] != '$')
            {
                columnName = null;

                return false;
            }

            StringBuilder tableNameBuilder = new StringBuilder();

            var index = 1;

            while (true)
            {
                var character = column.ColumnName[index++];

                if (character == '$')
                {
                    break;
                }
                else
                {
                    tableNameBuilder.Append(character);
                }
            }

            var tableName = tableNameBuilder.ToString();

            if (!dbConfiguration.Entities.TryGetValue(tableName, out entity))
            {
                throw new InvalidOperationException($"There is so entity mapped to the table '{tableName}'.");
            }

            columnName = column.ColumnName.Substring(index + 1, column.ColumnName.Length - index - 1);

            return true;
        }
    }
}