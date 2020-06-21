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

        internal Func<NpgsqlDataReader, Task<List<TEntity>>> GetOrCreateMaterializer(ReadOnlyCollection<NpgsqlDbColumn> columnSchema)
        {
            var cacheKeyBuilder = new HashCode();

            cacheKeyBuilder.Add(_entity.TableName);

            for (int i = 0; i < columnSchema.Count; i++)
            {
                var column = columnSchema[i];

                if (TryGetEntityOfTable(column, false, out var entity, out var columnName))
                {

                    cacheKeyBuilder.Add(entity.Entity.TableName);
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
                    var entities = new List<KeyValuePair<Entity, List<NpgsqlDbColumn>>>();
                    var columns = new List<NpgsqlDbColumn>();

                    entities.Add(new KeyValuePair<Entity, List<NpgsqlDbColumn>>(_entity, columns));

                    for (int i = 0; i < columnSchema.Count; i++)
                    {
                        var column = columnSchema[i];

                        if (TryGetEntityOfTable(column, false, out var entity, out var columnName))
                        {

                            columns = new List<NpgsqlDbColumn>
                            {
                                column
                            };

                            entities.Add(new KeyValuePair<Entity, List<NpgsqlDbColumn>>(_entity, columns)); // TODO: Entity should be from actual entity
                        }
                        else
                        {
                            columns.Add(column);
                        }
                    }

                    materializer = CreateMaterializer(entities);

                    _materializerCache.TryAdd(cacheKey, materializer);

                    return materializer;
                }
            }
        }

        private Func<NpgsqlDataReader, Task<List<TEntity>>> CreateMaterializer(List<KeyValuePair<Entity, List<NpgsqlDbColumn>>> entities)
        {
            var primaryEntity = entities[0];
            var primaryEntityListType = typeof(List<>).MakeGenericType(primaryEntity.Key.EntityType);

            var asyncStateMachineType = typeof(IAsyncStateMachine);
            var asyncMethodBuilderType = typeof(AsyncTaskMethodBuilder<>).MakeGenericType(primaryEntityListType);
            var npgsqlDataReaderType = typeof(NpgsqlDataReader);
            var taskAwaiterType = typeof(NpgsqlDataReader);
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

            if (entities.Count > 1)
            {
                var gernericDictionaryType = typeof(Dictionary<,>);

                for (int i = 0; i < entities.Count; i++)
                {
                    var entityWithColumns = entities[i];
                    var entity = entityWithColumns.Key;
                    var columns = entityWithColumns.Value;

                    var primaryKeyType = entity.GetPrimaryColumn().PropertyInfo.PropertyType;
                    var entityDictionaryType = gernericDictionaryType.MakeGenericType(primaryKeyType, entity.EntityType);

                    var lastEntityField = stateMachineTypeBuilder.DefineField("last" + entity.EntityName + i, entity.EntityType, FieldAttributes.Private);
                    var entityDictionaryField = stateMachineTypeBuilder.DefineField(entity.EntityName + "Dict" + i, entityDictionaryType, FieldAttributes.Private);

                    var primaryKeyLocal = moveNextMethodIL.DeclareLocal(primaryKeyType);
                    var tempEntityLocal = moveNextMethodIL.DeclareLocal(entity.EntityType);

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldnull);
                    moveNextMethodIL.Emit(OpCodes.Stfld, lastEntityField);

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Newobj, entityDictionaryType.GetConstructor(Type.EmptyTypes));
                    moveNextMethodIL.Emit(OpCodes.Stfld, entityDictionaryField);

                    var primaryKeyColumnScheme = columns[0];
                    var primaryKeyColumn = entity.GetColumn(primaryKeyColumnScheme.ColumnName);

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4, primaryKeyColumnScheme.ColumnOrdinal.Value);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryKeyColumn.DbValueRetriever);
                    iLGhostBodyGen.Emit(OpCodes.Stloc, primaryKeyLocal);

                    var lastEntityIfBody = moveNextMethodIL.DefineLabel();

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, lastEntityField);
                    iLGhostBodyGen.Emit(OpCodes.Brfalse, lastEntityIfBody);

                    iLGhostBodyGen.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, lastEntityField);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryKeyColumn.PropertyInfo.GetGetMethod());
                    iLGhostBodyGen.Emit(OpCodes.Beq, afterBodyLabel);

                    iLGhostBodyGen.MarkLabel(lastEntityIfBody);

                    var entityDictionaryIfBodyEnd = moveNextMethodIL.DefineLabel();

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, entityDictionaryField);
                    iLGhostBodyGen.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    iLGhostBodyGen.Emit(OpCodes.Ldloca, tempEntityLocal);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, entityDictionaryType.GetMethod("TryGetValue"));
                    iLGhostBodyGen.Emit(OpCodes.Brfalse, entityDictionaryIfBodyEnd);

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldloca, tempEntityLocal);
                    iLGhostBodyGen.Emit(OpCodes.Stfld, lastEntityField);
                    iLGhostBodyGen.Emit(OpCodes.Br, afterBodyLabel);

                    iLGhostBodyGen.MarkLabel(entityDictionaryIfBodyEnd);

                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Newobj, entity.EntityType.GetConstructor(Type.EmptyTypes));

                    iLGhostBodyGen.Emit(OpCodes.Dup);
                    iLGhostBodyGen.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryKeyColumn.PropertyInfo.GetSetMethod());

                    for (int k = 1; k < columns.Count; k++)
                    {
                        var columnScheme = columns[k];
                        var column = entity.GetColumn(columnScheme.ColumnName);

                        iLGhostBodyGen.Emit(OpCodes.Dup);
                        iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                        iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                        iLGhostBodyGen.Emit(OpCodes.Ldc_I4, columnScheme.ColumnOrdinal.Value);
                        iLGhostBodyGen.Emit(OpCodes.Callvirt, column.DbValueRetriever);
                        iLGhostBodyGen.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
                    }

                    if (entity.Relations is { })
                    {
                        for (int k = 0; k < entity.Relations.Count; k++)
                        {
                            var relation = entity.Relations[k];

                            if (relation.RelationType != RelationType.OneToMany)
                            {
                                continue;
                            }

                            iLGhostBodyGen.Emit(OpCodes.Dup);

                            iLGhostBodyGen.Emit(OpCodes.Newobj, relation.ForeignEntityColumn.PropertyType.GetConstructor(Type.EmptyTypes));
                            iLGhostBodyGen.Emit(OpCodes.Callvirt, relation.ForeignEntityColumn.GetSetMethod());
                        }
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

                    iLSetNullGhostGen.Emit(OpCodes.Ldarg_0);
                    iLSetNullGhostGen.Emit(OpCodes.Ldnull);
                    iLSetNullGhostGen.Emit(OpCodes.Stfld, lastEntityField);

                    iLSetNullGhostGen.Emit(OpCodes.Ldarg_0);
                    iLSetNullGhostGen.Emit(OpCodes.Ldnull);
                    iLSetNullGhostGen.Emit(OpCodes.Stfld, entityDictionaryField);
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
                    var column = _entity.GetColumn(columnScheme.ColumnName);

                    iLGhostBodyGen.Emit(OpCodes.Dup);
                    iLGhostBodyGen.Emit(OpCodes.Ldarg_0);
                    iLGhostBodyGen.Emit(OpCodes.Ldfld, dataReaderField);
                    iLGhostBodyGen.Emit(OpCodes.Ldc_I4, columnScheme.ColumnOrdinal.Value);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, column.DbValueRetriever);
                    iLGhostBodyGen.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
                }

                iLGhostBodyGen.Emit(OpCodes.Callvirt, primaryEntityListType.GetMethod("Add"));
            }


            // -- End of actual Local and Field Assignment

            var beforeBodyLabel = moveNextMethodIL.DefineLabel();

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

        private bool TryGetEntityOfTable(NpgsqlDbColumn column, bool isFirst, out ForeignEntity? entity,
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

            if (isFirst)
            {
                if (_entity.TableName != tableName)
                {
                    columnName = null;

                    return false;
                }
            }
            else
            {
                if (_entity.Relations is null || !_entity.Relations.TryGetValue(tableName, out entity)
                ) // TODO: Use tables/entities form before defined JoinInformation
                {
                    columnName = null;

                    return false;
                }
            }

            columnName = column.ColumnName.Substring(index, column.ColumnName.Length - index);

            return true;
        }
    }
}