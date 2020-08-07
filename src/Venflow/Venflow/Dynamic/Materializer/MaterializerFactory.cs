using Npgsql;
using Npgsql.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Commands;
using Venflow.Dynamic.IL;
using Venflow.Dynamic.Mat;
using Venflow.Dynamic.Proxies;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Materializer
{
    internal class MaterializerFactory<TEntity> where TEntity : class, new()
    {
        private readonly Entity<TEntity> _entity;
        private readonly Dictionary<int, Delegate> _materializerCache;
        private readonly object _materializerLock;

        internal MaterializerFactory(Entity<TEntity> entity)
        {
            _entity = entity;

            _materializerCache = new Dictionary<int, Delegate>();
            _materializerLock = new object();
        }

        internal Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> GetOrCreateMaterializer<TReturn>(JoinBuilderValues? joinBuilderValues, ReadOnlyCollection<NpgsqlDbColumn> columnSchema, bool changeTracking) where TReturn : class, new()
        {
            var cacheKeyBuilder = new HashCode();

            cacheKeyBuilder.Add(typeof(TReturn));

            var hasJoins = joinBuilderValues is { };

            var columnIndex = 0;

            if (hasJoins)
            {
                var joinIndex = 0;

                Entity nextJoin = _entity;
                string? nextJoinPKName = _entity.PrimaryColumn.ColumnName;

                for (; columnIndex < columnSchema.Count; columnIndex++)
                {
                    var columnName = columnSchema[columnIndex].ColumnName;

                    if (columnName == nextJoinPKName)
                    {
                        cacheKeyBuilder.Add(nextJoin.EntityName);

                        if (joinBuilderValues.Joins.Count == joinIndex)
                            break;

                        nextJoin = joinBuilderValues.Joins[joinIndex].Join.RightEntity;
                        nextJoinPKName = nextJoin.GetPrimaryColumn().ColumnName;

                        joinIndex++;
                    }

                    cacheKeyBuilder.Add(columnName);
                }
            }
            else
            {
                cacheKeyBuilder.Add(_entity.TableName);
            }

            for (; columnIndex < columnSchema.Count; columnIndex++)
            {
                cacheKeyBuilder.Add(columnSchema[columnIndex].ColumnName);
            }

            cacheKeyBuilder.Add(changeTracking);

            var cacheKey = cacheKeyBuilder.ToHashCode();

            lock (_materializerLock)
            {
                if (_materializerCache.TryGetValue(cacheKey, out var tempMaterializer))
                {
                    return (Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>)tempMaterializer;
                }
                else
                {
                    QueryEntityHolder[] generatedEntities;

                    if (hasJoins)
                    {
                        var sourceCompiler = new MaterializerSourceCompiler(joinBuilderValues);

                        sourceCompiler.Compile();

                        generatedEntities = sourceCompiler.GenerateSortedEntities();
                    }
                    else
                    {
                        generatedEntities = new[] { new QueryEntityHolder(_entity, 0) };
                    }

                    var entities = new List<(QueryEntityHolder, List<(EntityColumn, int)>)>();
                    List<(EntityColumn, int)> columns = default;

                    var joinIndex = 1;

                    QueryEntityHolder nextJoin = generatedEntities[0];
                    QueryEntityHolder currentJoin = generatedEntities[0];

                    var nextJoinPKName = _entity.PrimaryColumn.ColumnName;

                    for (columnIndex = 0; columnIndex < columnSchema.Count; columnIndex++)
                    {
                        var column = columnSchema[columnIndex];

                        if (column.ColumnName == nextJoinPKName)
                        {
                            if (columnIndex > 0)
                                currentJoin = nextJoin;

                            columns = new List<(EntityColumn, int)>();

                            entities.Add((nextJoin, columns));

                            if (hasJoins && joinIndex < generatedEntities.Length)
                            {
                                nextJoin = generatedEntities[joinIndex];
                                nextJoinPKName = nextJoin.Entity.GetPrimaryColumn().ColumnName;

                                var currentJoinColumnCount = currentJoin.Entity.GetColumnCount();

                                for (int i = currentJoin.Entity.GetRegularColumnOffset(); i < currentJoinColumnCount; i++)
                                {
                                    var currentJoinColumn = currentJoin.Entity.GetColumn(i);

                                    if (currentJoinColumn.ColumnName == nextJoinPKName)
                                    {
                                        throw new InvalidOperationException($"The entity '{currentJoin.Entity.EntityName}' defines the column '{currentJoinColumn.ColumnName}' which can't have the same name, as the joining entity's '{nextJoin.Entity.EntityName}' primary key '{nextJoinPKName}'.");
                                    }
                                }

                                joinIndex++;
                            }
                        }

                        if (!currentJoin.Entity.TryGetColumn(column.ColumnName, out var entityColumn))
                        {
                            throw new InvalidOperationException($"The column '{column.ColumnName}' on entity '{currentJoin.Entity.EntityName}' does not exist.");
                        }

                        columns.Add((entityColumn, column.ColumnOrdinal.Value));
                    }

                    if (hasJoins)
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
                    else if (entities.Count > 1)
                    {
                        throw new InvalidOperationException("The result set contained multiple tables, however the query was configured to only expect one. Try specifying the tables you are joining with JoinWith, while declaring the query.");
                    }

                    return new Mat.MaterializerFactory(_entity).CreateMaterializer<TReturn>(entities, changeTracking);

                    //                    var materializer = CreateMaterializer<TReturn>(joinBuilderValues, entities, changeTracking);

                    //#if NET48
                    //                                        _materializerCache.Add(cacheKey, materializer);
                    //#else
                    //                    _materializerCache.TryAdd(cacheKey, materializer);
                    //#endif

                    //                    return materializer;
                }
            }
        }

        private Func<NpgsqlDataReader, Task<TReturn>> CreateMaterializer<TReturn>(JoinBuilderValues? joinBuilderValues, List<KeyValuePair<Entity, List<KeyValuePair<string, int>>>> entities, bool changeTracking) where TReturn : class, new()
        {
            var primaryEntity = entities[0];
            var returnType = typeof(TReturn);
            var isSingleResult = returnType == primaryEntity.Key.EntityType;
            var genericListType = typeof(List<>);

            var primaryEntityType = returnType;

            var asyncStateMachineType = typeof(IAsyncStateMachine);
            var asyncMethodBuilderType = typeof(AsyncTaskMethodBuilder<>).MakeGenericType(returnType);
            var npgsqlDataReaderType = typeof(NpgsqlDataReader);
            var taskAwaiterType = typeof(TaskAwaiter<bool>);
            var intType = typeof(int);
            var boolType = typeof(bool);
            var taskBoolType = typeof(Task<bool>);
            var exceptionType = typeof(Exception);
            var genericChangeTracker = typeof(ChangeTracker<>);

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
            var primaryEntityLocal = moveNextMethodIL.DeclareLocal(primaryEntityType);

            var endOfMethodLabel = moveNextMethodIL.DefineLabel();

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, stateField);
            moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);

            var exceptionBlock = moveNextMethodIL.BeginExceptionBlock();
            var ifGotoLabel = moveNextMethodIL.DefineLabel();

            moveNextMethodIL.Emit(OpCodes.Ldloc, stateLocal);
            moveNextMethodIL.Emit(OpCodes.Brfalse, ifGotoLabel);

            // -- Actual Local and Field Assignment

            FieldBuilder primaryEntityField;

            if (isSingleResult)
            {
                primaryEntityField = stateMachineTypeBuilder.DefineField(primaryEntity.Key.EntityName, primaryEntityType, FieldAttributes.Private);
            }
            else
            {
                primaryEntityField = stateMachineTypeBuilder.DefineField(primaryEntity.Key.EntityName + "List", primaryEntityType, FieldAttributes.Private);

                moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                moveNextMethodIL.Emit(OpCodes.Newobj, primaryEntityType.GetConstructor(Type.EmptyTypes));
                moveNextMethodIL.Emit(OpCodes.Stfld, primaryEntityField);
            }

            var iLSetNullGhostGen = new ILGhostGenerator();

            iLSetNullGhostGen.Emit(OpCodes.Ldarg_0);
            iLSetNullGhostGen.Emit(OpCodes.Ldnull);
            iLSetNullGhostGen.Emit(OpCodes.Stfld, primaryEntityField);

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

                    FieldBuilder? lastEntityField = default;
                    FieldBuilder? entityDictionaryField = default;

                    if (!isSingleResult || i > 0)
                    {
                        lastEntityField = stateMachineTypeBuilder.DefineField("last" + entity.EntityName + i, entity.EntityType, FieldAttributes.Private);
                        entityDictionaryField = stateMachineTypeBuilder.DefineField(entity.EntityName + "Dict" + i, entityDictionaryType, FieldAttributes.Private);
                    }

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

                    if (!isSingleResult || i > 0)
                    {
                        moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        moveNextMethodIL.Emit(OpCodes.Ldnull);
                        moveNextMethodIL.Emit(OpCodes.Stfld, lastEntityField);

                        moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        moveNextMethodIL.Emit(OpCodes.Newobj, entityDictionaryType.GetConstructor(Type.EmptyTypes));
                        moveNextMethodIL.Emit(OpCodes.Stfld, entityDictionaryField);
                    }

                    var primaryKeyColumnScheme = columns[0];
                    var primaryKeyColumn = entity.GetColumn(primaryKeyColumnScheme.Key);

                    if (nextIfStartLabel.HasValue)
                    {
                        iLGhostBodyGen.MarkLabel(nextIfStartLabel.Value);
                    }

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4, primaryKeyColumnScheme.Value);
                    GetColumnMaterializer(iLGhostBodyGen, primaryKeyColumn);
                    iLGhostBodyGen.Emit(OpCodes.Stloc, primaryKeyLocal);

                    nextIfStartLabel = moveNextMethodIL.DefineLabel();

                    if (!isSingleResult || i > 0)
                    {
                        var lastEntityIfBody = moveNextMethodIL.DefineLabel();

                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldfld, lastEntityField);
                        iLGhostBodyGen.Emit(OpCodes.Brfalse, lastEntityIfBody);

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
                        iLGhostBodyGen.Emit(OpCodes.Ldloc, entityLocal);
                        iLGhostBodyGen.Emit(OpCodes.Stfld, lastEntityField);
                        iLGhostBodyGen.Emit(OpCodes.Br, nextIfStartLabel.Value);

                        iLGhostBodyGen.MarkLabel(entityDictionaryIfBodyEnd);
                    }
                    else
                    {
                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldfld, primaryEntityField);
                        iLGhostBodyGen.Emit(OpCodes.Brtrue, nextIfStartLabel.Value);
                    }

                    var hasChangeTracking = changeTracking && entity.ProxyEntityType is { };

                    LocalBuilder? changeTrackerLocal = default;
                    Type? changeTracker = default;

                    if (hasChangeTracking)
                    {
                        changeTracker = genericChangeTracker.MakeGenericType(entity.EntityType);
                        changeTrackerLocal = moveNextMethodIL.DeclareLocal(entity.ProxyEntityType);

                        iLGhostBodyGen.Emit(OpCodes.Ldc_I4, columns.Count);
                        iLGhostBodyGen.Emit(OpCodes.Ldc_I4_0);
                        iLGhostBodyGen.Emit(OpCodes.Newobj, changeTracker.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { intType, boolType }, null));
                        iLGhostBodyGen.Emit(OpCodes.Stloc, changeTrackerLocal);
                    }

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);

                    if (hasChangeTracking)
                    {
                        iLGhostBodyGen.Emit(OpCodes.Ldloc, changeTrackerLocal);
                        iLGhostBodyGen.Emit(OpCodes.Newobj, entity.ProxyEntityType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { changeTracker }, null));
                    }
                    else
                    {
                        iLGhostBodyGen.Emit(OpCodes.Newobj, entity.EntityType.GetConstructor(Type.EmptyTypes));
                    }

                    iLGhostBodyGen.Emit(OpCodes.Dup);
                    iLGhostBodyGen.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryKeyColumn.PropertyInfo.GetSetMethod());

                    for (int k = 1; k < columns.Count; k++)
                    {
                        var columnScheme = columns[k];

                        if (!entity.TryGetColumn(columnScheme.Key, out var column))
                        {
                            throw new InvalidOperationException($"The query returned the column '{columnScheme.Key}' however the entity '{entity.EntityName}' doesn't contain a matching property.");
                        }

                        iLGhostBodyGen.Emit(OpCodes.Dup);
                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                        iLGhostBodyGen.Emit(OpCodes.Ldc_I4, columnScheme.Value);
                        GetColumnMaterializer(iLGhostBodyGen, column);
                        iLGhostBodyGen.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
                    }

                    if (entity.Relations is { } && entity.Relations.Count > 0)
                    {
                        EntityRelationAssignment? entityRelationAssignment = null;

                        if (shouldCheckForChange)
                        {
                            entityRelationAssignment = new EntityRelationAssignment(isSingleResult && i == 0 ? primaryEntityField : lastEntityField, hasEntityChangedField);
                            relationAssignments.Add(entityRelationAssignment);
                        }

                        for (int k = 0; k < entity.Relations.Count; k++)
                        {
                            var relation = entity.Relations[k];

                            if (relation.LeftNavigationProperty is null)
                                continue;

                            lastEntityFieldDictionary.Add(entity.EntityName + relation.RightEntity.EntityName + relation.LeftNavigationProperty.Name, isSingleResult && i == 0 ? primaryEntityField : lastEntityField);

                            if (relation.RelationType != RelationType.OneToMany)
                            {
                                if (shouldCheckForChange && relation.RightNavigationProperty is { })
                                {
                                    entityRelationAssignment.Relations.Add(new RelationAssignmentInformation(relation, relation.RightEntity.EntityName + entity.EntityName + relation.RightNavigationProperty.Name));
                                }

                                continue;
                            }

                            iLGhostBodyGen.Emit(OpCodes.Dup);

                            iLGhostBodyGen.Emit(OpCodes.Newobj, relation.LeftNavigationProperty.PropertyType.GetConstructor(Type.EmptyTypes));
                            iLGhostBodyGen.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetSetMethod());
                        }
                    }
                    if (!isSingleResult || i > 0)
                    {
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
                            iLGhostBodyGen.Emit(OpCodes.Ldfld, primaryEntityField);
                            iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                            iLGhostBodyGen.Emit(OpCodes.Ldfld, lastEntityField);
                            iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryEntityType.GetMethod("Add"));
                        }
                    }
                    else
                    {
                        iLGhostBodyGen.Emit(OpCodes.Stfld, primaryEntityField);
                    }

                    if (hasChangeTracking)
                    {
                        iLGhostBodyGen.Emit(OpCodes.Ldloc, changeTrackerLocal);
                        iLGhostBodyGen.Emit(OpCodes.Ldc_I4_1);
                        iLGhostBodyGen.Emit(OpCodes.Callvirt, changeTracker.GetProperty("TrackChanges", BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true));
                    }

                    if (shouldCheckForChange)
                    {
                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldc_I4_1);
                        iLGhostBodyGen.Emit(OpCodes.Stfld, hasEntityChangedField);
                    }

                    if (!isSingleResult || i > 0)
                    {
                        iLSetNullGhostGen.Emit(OpCodes.Ldarg_0);
                        iLSetNullGhostGen.Emit(OpCodes.Ldnull);
                        iLSetNullGhostGen.Emit(OpCodes.Stfld, lastEntityField);

                        iLSetNullGhostGen.Emit(OpCodes.Ldarg_0);
                        iLSetNullGhostGen.Emit(OpCodes.Ldnull);
                        iLSetNullGhostGen.Emit(OpCodes.Stfld, entityDictionaryField);
                    }
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

                            if (relationAssignment.EntityRelation.LeftNavigationProperty is { })
                            {
                                iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                                iLGhostBodyGen.Emit(OpCodes.Ldfld, entityRelationAssignment.LastLeftEntity);
                                iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                                iLGhostBodyGen.Emit(OpCodes.Ldfld, lastRightEntityField);
                                iLGhostBodyGen.Emit(OpCodes.Callvirt, relationAssignment.EntityRelation.LeftNavigationProperty.GetSetMethod());
                            }
                        }
                    }
                }
            }
            else
            {
                var columns = entities[0].Value;

                var hasChangeTracking = changeTracking && _entity.ProxyEntityType is { };

                LocalBuilder? changeTrackerLocal = default;
                Type? changeTracker = default;

                if (hasChangeTracking)
                {
                    changeTracker = genericChangeTracker.MakeGenericType(_entity.EntityType);
                    changeTrackerLocal = moveNextMethodIL.DeclareLocal(_entity.ProxyEntityType);

                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4, columns.Count);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4_0);
                    iLGhostBodyGen.Emit(OpCodes.Newobj, changeTracker.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { intType, boolType }, null));
                    iLGhostBodyGen.Emit(OpCodes.Stloc, changeTrackerLocal);
                }

                iLGhostBodyGen.Emit(OpCodes.Ldarg_0);

                if (!isSingleResult)
                {
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, primaryEntityField);
                }

                if (hasChangeTracking)
                {
                    iLGhostBodyGen.Emit(OpCodes.Ldloc, changeTrackerLocal);
                    iLGhostBodyGen.Emit(OpCodes.Newobj, _entity.ProxyEntityType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { changeTracker }, null));
                }
                else
                {
                    iLGhostBodyGen.Emit(OpCodes.Newobj, _entity.EntityType.GetConstructor(Type.EmptyTypes));
                }

                for (int k = 0; k < columns.Count; k++)
                {
                    var columnScheme = columns[k];

                    if (!_entity.TryGetColumn(columnScheme.Key, out var column))
                    {
                        throw new InvalidOperationException($"The query returned the column '{columnScheme.Key}' however the entity '{_entity.EntityName}' doesn't contain a matching property.");
                    }

                    iLGhostBodyGen.Emit(OpCodes.Dup);
                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4, columnScheme.Value);
                    GetColumnMaterializer(iLGhostBodyGen, column);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
                }

                if (isSingleResult)
                {
                    iLGhostBodyGen.Emit(OpCodes.Stfld, primaryEntityField);
                }
                else
                {
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryEntityType.GetMethod("Add"));
                }

                if (hasChangeTracking)
                {
                    iLGhostBodyGen.Emit(OpCodes.Ldloc, changeTrackerLocal);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4_1);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, changeTracker.GetProperty("TrackChanges", BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true));
                }
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
                npgsqlDataReaderType.GetMethod("ReadAsync", Type.EmptyTypes)); // TODO: Add Cancellation Token
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
            moveNextMethodIL.Emit(OpCodes.Ldfld, primaryEntityField);
            moveNextMethodIL.Emit(OpCodes.Stloc, primaryEntityLocal);

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
            moveNextMethodIL.Emit(OpCodes.Ldloc, primaryEntityLocal);

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
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, typeof(Task<>).MakeGenericType(returnType),
                new[] { npgsqlDataReaderType });

            materializeMethod.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(AsyncStateMachineAttribute).GetConstructor(new[] { typeof(Type) }),
                new[] { stateMachineTypeBuilder }));

            var materializeMethodIL = materializeMethod.GetILGenerator();

            materializeMethodIL.DeclareLocal(stateMachineTypeBuilder);

            // Create and execute the StateMachine

            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Call, asyncMethodBuilderType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static));
            materializeMethodIL.Emit(OpCodes.Stfld, methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldarg_0);
            materializeMethodIL.Emit(OpCodes.Stfld, dataReaderField);
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

            return (Func<NpgsqlDataReader, Task<TReturn>>)materializerType.GetMethod("MaterializeAsync").CreateDelegate(typeof(Func<NpgsqlDataReader, Task<TReturn>>));
        }

        private void GetColumnMaterializer(ILGhostGenerator iLGenerator, EntityColumn column)
        {
            if (column.IsNullableReferenceType)
            {
                var valueRetriever = typeof(NpgsqlDataReaderExtensions).GetMethod("GetValueOrDefault", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(column.PropertyInfo.PropertyType);

                iLGenerator.Emit(OpCodes.Call, valueRetriever);
            }
            else
            {

                var valueRetriever = TypeCache.NpgsqlDataReader!.GetMethod("GetFieldValue", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(column.PropertyInfo.PropertyType);

                iLGenerator.Emit(OpCodes.Callvirt, valueRetriever);
            }
        }
    }
}