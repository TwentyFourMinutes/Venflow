using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Dynamic.IL;
using Venflow.Dynamic.Proxies;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Materializer
{
    internal class MaterializerFactoryCompiler
    {
        private FieldBuilder _dataReaderField;

        private FieldBuilder _methodBuilderField;
        private FieldBuilder _cancellationTokenField;
        private FieldBuilder _boolTaskAwaiterField;
        private LocalBuilder _boolTaskAwaiterLocal;

        private LocalBuilder _defaultExceptionLocal;

        private FieldBuilder _stateField;
        private LocalBuilder _stateLocal;
        private Type _returnType;

        private readonly TypeBuilder _materializerTypeBuilder;
        private readonly TypeBuilder _stateMachineTypeBuilder;
        private readonly MethodBuilder _moveNextMethod;
        private readonly ILGenerator _moveNextMethodIL;

        private readonly Type _intType = typeof(int);
        private readonly Type _dataReaderType = typeof(NpgsqlDataReader);

        private readonly Entity _rootEntity;

        internal MaterializerFactoryCompiler(Entity rootEntity)
        {
            _materializerTypeBuilder = TypeFactory.GetNewMaterializerBuilder(rootEntity.EntityName,
                TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Abstract |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            _stateMachineTypeBuilder = _materializerTypeBuilder.DefineNestedType("StateMachine",
                TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, typeof(ValueType),
                new[] { typeof(IAsyncStateMachine) });

            _moveNextMethod = _stateMachineTypeBuilder.DefineMethod("MoveNext",
                 MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig |
                 MethodAttributes.NewSlot | MethodAttributes.Virtual);

            _moveNextMethodIL = _moveNextMethod.GetILGenerator();

            _rootEntity = rootEntity;
        }


        internal Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> CreateMaterializer<TReturn>(List<(QueryEntityHolder, List<(EntityColumn, int)>)> entities, bool changeTracking) where TReturn : class, new()
        {
            _returnType = typeof(TReturn);

            bool isSingleResult = _returnType == _rootEntity.EntityType;

            _stateField = _stateMachineTypeBuilder.DefineField("_stateField", _intType, FieldAttributes.Public);
            _stateLocal = _moveNextMethodIL.DeclareLocal(_intType);

            _dataReaderField = _stateMachineTypeBuilder.DefineField("_stateField", _dataReaderType, FieldAttributes.Public);

            _methodBuilderField = _stateMachineTypeBuilder.DefineField("_methodBuilder", typeof(AsyncTaskMethodBuilder<>).MakeGenericType(_returnType), FieldAttributes.Public);
            _cancellationTokenField = _stateMachineTypeBuilder.DefineField("_canellationToken", typeof(CancellationToken), FieldAttributes.Public);
            _boolTaskAwaiterField = _stateMachineTypeBuilder.DefineField("_boolTaskAwaiter", typeof(TaskAwaiter<bool>), FieldAttributes.Private);
            _boolTaskAwaiterLocal = _moveNextMethodIL.DeclareLocal(_boolTaskAwaiterField.FieldType);

            _defaultExceptionLocal = _moveNextMethodIL.DeclareLocal(typeof(Exception));

            if (isSingleResult)
            {
                if (entities.Count == 1)
                {
                    CreateSingleNoRelationMaterializer(entities[0].Item2, changeTracking && _rootEntity.ProxyEntityType is { });
                }
                else
                {
                    CreateSingleRelationMaterializer(entities, changeTracking);
                }
            }
            else
            {
                if (entities.Count == 1)
                {
                    CreateBatchNoRelationMaterializer(entities[0].Item2, changeTracking && _rootEntity.ProxyEntityType is { });
                }
                else
                {
                    CreateBatchRelationMaterializer(entities, changeTracking);
                }
            }

            _stateMachineTypeBuilder.DefineMethodOverride(_moveNextMethod, typeof(IAsyncStateMachine).GetMethod("MoveNext"));

            var setStateMachineMethod = _stateMachineTypeBuilder.DefineMethod("SetStateMachine",
                MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual, null, new[] { typeof(IAsyncStateMachine) });
            var setStateMachineMethodIL = setStateMachineMethod.GetILGenerator();

            setStateMachineMethodIL.Emit(OpCodes.Ldarg_0);
            setStateMachineMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            setStateMachineMethodIL.Emit(OpCodes.Ldarg_1);
            setStateMachineMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetStateMachine"));
            setStateMachineMethodIL.Emit(OpCodes.Ret);

            _stateMachineTypeBuilder.DefineMethodOverride(setStateMachineMethod, typeof(IAsyncStateMachine).GetMethod("SetStateMachine"));

            var materializeMethod = _materializerTypeBuilder.DefineMethod("MaterializeAsync",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, typeof(Task<>).MakeGenericType(_returnType),
                new[] { _dataReaderType, _cancellationTokenField.FieldType });

            materializeMethod.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(AsyncStateMachineAttribute).GetConstructor(new[] { typeof(Type) }),
                new[] { _stateMachineTypeBuilder }));

            var materializeMethodIL = materializeMethod.GetILGenerator();

            materializeMethodIL.DeclareLocal(_stateMachineTypeBuilder);

            // Create and execute the StateMachine

            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static));
            materializeMethodIL.Emit(OpCodes.Stfld, _methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldarg_0);
            materializeMethodIL.Emit(OpCodes.Stfld, _dataReaderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldarg_1);
            materializeMethodIL.Emit(OpCodes.Stfld, _cancellationTokenField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldc_I4_M1);
            materializeMethodIL.Emit(OpCodes.Stfld, _stateField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Call,
               _methodBuilderField.FieldType.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(_stateMachineTypeBuilder));
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetProperty("Task").GetGetMethod());

            materializeMethodIL.Emit(OpCodes.Ret);

            _stateMachineTypeBuilder.CreateType();
            var materializerType = _materializerTypeBuilder.CreateType();

            return (Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>)materializerType.GetMethod("MaterializeAsync").CreateDelegate(typeof(Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>));

        }

        private void CreateSingleNoRelationMaterializer(List<(EntityColumn, int)> dbColumns, bool changeTracking)
        {
            var resultLocal = _moveNextMethodIL.DeclareLocal(_returnType);

            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var endOfCatchLabel = _moveNextMethodIL.DefineLabel();

            // Assign the state field to the state local
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _stateLocal);

            // Start try block
            _moveNextMethodIL.BeginExceptionBlock();

            // if state zero goto await unsafe
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _stateLocal);

            var switchBuilder = new ILSwitchBuilder(_moveNextMethodIL, 1);

            var asyncGenerator = new ILAsyncGenerator(_moveNextMethodIL, switchBuilder, _methodBuilderField, _stateField, _stateLocal, endOfMethodLabel, _stateMachineTypeBuilder);

            _moveNextMethodIL.Emit(OpCodes.Brfalse, switchBuilder.GetLabels()[0]);

            // if no rows return
            var afterNoRowsIfBody = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetProperty("HasRows").GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterNoRowsIfBody);

            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stloc, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            _moveNextMethodIL.MarkLabel(afterNoRowsIfBody);

            // Call ReadAsync(cancellationToken) on dataReader
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }));

            asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), _boolTaskAwaiterLocal, _boolTaskAwaiterField);

            var endOfNoRowIfBody = _moveNextMethodIL.DefineLabel();
            _moveNextMethodIL.Emit(OpCodes.Brtrue, endOfNoRowIfBody);

            // return null
            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            _moveNextMethodIL.MarkLabel(endOfNoRowIfBody);

            // create new Entity

            CreateEntity(_rootEntity, dbColumns, changeTracking);

            // return result
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, resultLocal);

            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            // End of try block

            // Start of catch block
            _moveNextMethodIL.BeginCatchBlock(_defaultExceptionLocal.LocalType);

            // Set state and return exception
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _defaultExceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _defaultExceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            // End of catch block
            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfCatchLabel);

            // Set state and return result
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetResult"));

            // End of method
            _moveNextMethodIL.MarkLabel(endOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);
        }

        private void CreateSingleRelationMaterializer(List<(QueryEntityHolder, List<(EntityColumn, int)>)> entities, bool changeTracking)
        {
            var resultField = _stateMachineTypeBuilder.DefineField("_result", _returnType, FieldAttributes.Private);
            var resultLocal = _moveNextMethodIL.DeclareLocal(_returnType);

            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var endOfCatchLabel = _moveNextMethodIL.DefineLabel();
            var loopBodyLabel = _moveNextMethodIL.DefineLabel();
            var loopConditionLabel = _moveNextMethodIL.DefineLabel();

            // Assign the state field to the state local
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _stateLocal);

            // Start try block
            _moveNextMethodIL.BeginExceptionBlock();

            // if state zero goto await unsafe

            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _stateLocal);

            var switchBuilder = new ILSwitchBuilder(_moveNextMethodIL, 1);

            var asyncGenerator = new ILAsyncGenerator(_moveNextMethodIL, switchBuilder, _methodBuilderField, _stateField, _stateLocal, endOfMethodLabel, _stateMachineTypeBuilder);

            _moveNextMethodIL.Emit(OpCodes.Brfalse, switchBuilder.GetLabels()[0]);

            // if no rows return
            var afterNoRowsIfBody = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetProperty("HasRows").GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterNoRowsIfBody);

            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stloc, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            _moveNextMethodIL.MarkLabel(afterNoRowsIfBody);

            // Create result field instance

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);

            var entityDictionaries = new Dictionary<int, FieldBuilder>(entities.Count - 1);
            var entityLastTypes = new Dictionary<int, FieldBuilder>(entities.Count - 1);

            var relationDictionaries = new Dictionary<uint, FieldBuilder>(entities.Count - 1);
            var lastRelationMaps = new Dictionary<uint, FieldBuilder>(entities.Count);

            var dictionaryType = typeof(Dictionary<,>);
            var hashSetType = typeof(HashSet<>);

            for (int i = 1; i < entities.Count; i++)
            {
                var entityHolder = entities[i];
                var entity = entityHolder.Item1.Entity;

                // Add dictionary field
                var entityDictionaryField = _stateMachineTypeBuilder.DefineField("_" + entity.EntityName + entityHolder.Item1.Id, dictionaryType.MakeGenericType(entity.GetPrimaryColumn().PropertyInfo.PropertyType, entity.EntityType), FieldAttributes.Private);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Newobj, entityDictionaryField.FieldType.GetConstructor(Type.EmptyTypes));
                _moveNextMethodIL.Emit(OpCodes.Stfld, entityDictionaryField);

                entityDictionaries.Add(entityHolder.Item1.Id, entityDictionaryField);

                // Add lastEntity field

                var lastEntityField = _stateMachineTypeBuilder.DefineField("_last" + entity.EntityName + entityHolder.Item1.Id, entity.EntityType, FieldAttributes.Private);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, lastEntityField);

                entityLastTypes.Add(entityHolder.Item1.Id, lastEntityField);

                for (int relationIndex = 0; relationIndex < entityHolder.Item1.ForeignAssignedRelations.Count; relationIndex++)
                {
                    var relation = entityHolder.Item1.ForeignAssignedRelations[relationIndex].Item1;

                    if (relation.RelationType != RelationType.ManyToOne)
                        continue;

                    // Add relationDictionary field

                    var relationDictionaryName = entity.EntityName + "_Relation" + entityHolder.Item1.Id + "_" + relation.RelationId;

                    var relationMap = hashSetType.MakeGenericType(relation.RightEntity.GetPrimaryColumn().PropertyInfo.PropertyType);

                    var relationDictionaryField = _stateMachineTypeBuilder.DefineField("_" + relationDictionaryName, dictionaryType.MakeGenericType(entity.GetPrimaryColumn().PropertyInfo.PropertyType, relationMap), FieldAttributes.Private);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Newobj, relationDictionaryField.FieldType.GetConstructor(Type.EmptyTypes));
                    _moveNextMethodIL.Emit(OpCodes.Stfld, relationDictionaryField);

                    relationDictionaries.Add(relation.RelationId, relationDictionaryField);

                    // Add lastRelationMap field

                    var lastRelationMapField = _stateMachineTypeBuilder.DefineField("_last" + relationDictionaryName + "_Map", relationMap, FieldAttributes.Private);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldnull);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                    lastRelationMaps.Add(relation.RelationId, lastRelationMapField);
                }
            }

            var primaryEntityHolder = entities[0];
            var primaryEntity = primaryEntityHolder.Item1.Entity;

            for (int relationIndex = 0; relationIndex < primaryEntityHolder.Item1.ForeignAssignedRelations.Count; relationIndex++)
            {
                var relation = primaryEntityHolder.Item1.ForeignAssignedRelations[relationIndex].Item1;

                if (relation.RelationType != RelationType.ManyToOne)
                    continue;

                // Add lastRelationMap field

                var relationDictionaryName = primaryEntity.EntityName + "_Relation" + primaryEntityHolder.Item1.Id + "_" + relation.RelationId;

                var relationMap = hashSetType.MakeGenericType(relation.RightEntity.GetPrimaryColumn().PropertyInfo.PropertyType);

                var lastRelationMapField = _stateMachineTypeBuilder.DefineField("_last" + relationDictionaryName + "_Map", relationMap, FieldAttributes.Private);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Newobj, lastRelationMapField.FieldType.GetConstructor(Type.EmptyTypes));
                _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                lastRelationMaps.Add(relation.RelationId, lastRelationMapField);
            }

            for (int relationIndex = 0; relationIndex < primaryEntity.Relations.Count; relationIndex++)
            {
                var relation = primaryEntity.Relations[relationIndex];

                if (relation.RelationType != RelationType.OneToMany ||
                    !lastRelationMaps.TryGetValue(relation.RelationId, out var lastRelationMapField))
                    continue;

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Newobj, lastRelationMapField.FieldType.GetConstructor(Type.EmptyTypes));
                _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);
            }

            // setIsFirstRow to true
            var isFirstRowField = _stateMachineTypeBuilder.DefineField("isFirstRow", typeof(bool), FieldAttributes.Private);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
            _moveNextMethodIL.Emit(OpCodes.Stfld, isFirstRowField);

            _moveNextMethodIL.Emit(OpCodes.Br, loopConditionLabel);

            _moveNextMethodIL.MarkLabel(loopBodyLabel);
            var setNullGhostIL = new ILGhostGenerator();

            // Set result field to null
            setNullGhostIL.Emit(OpCodes.Ldarg_0);
            setNullGhostIL.Emit(OpCodes.Ldnull);
            setNullGhostIL.Emit(OpCodes.Stfld, resultField);

            entityLastTypes.Add(primaryEntityHolder.Item1.Id, resultField);

            // create new Entity
            // Check if result if first row null
            var afterEntityGenerationIfBody = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, isFirstRowField);
            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterEntityGenerationIfBody);

            // Instantiate the entity

            if (!primaryEntityHolder.Item1.HasRelations)
            {
                // Set is first row to false
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Stfld, isFirstRowField);
            }

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);

            CreateEntity(primaryEntity, primaryEntityHolder.Item2, changeTracking && primaryEntity.ProxyEntityType is { });

            for (int i = primaryEntityHolder.Item1.InitializeNavigations.Count - 1; i >= 0; i--)
            {
                var initializeNavigation = primaryEntityHolder.Item1.InitializeNavigations[i];

                _moveNextMethodIL.Emit(OpCodes.Dup);
                _moveNextMethodIL.Emit(OpCodes.Newobj, typeof(List<>).MakeGenericType(new[] { initializeNavigation.LeftNavigationProperty.PropertyType.GetGenericArguments()[0] }).GetConstructor(Type.EmptyTypes));
                _moveNextMethodIL.Emit(OpCodes.Callvirt, initializeNavigation.LeftNavigationProperty.GetSetMethod());
            }

            // return result
            _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);

            _moveNextMethodIL.MarkLabel(afterEntityGenerationIfBody);

            var changedLocals = new Dictionary<int, LocalBuilder>(entities.Count);

            for (int entityIndex = 1; entityIndex < entities.Count; entityIndex++)
            {
                var entityHolder = entities[entityIndex];
                var entity = entityHolder.Item1.Entity;
                var lastEntityField = entityLastTypes[entityHolder.Item1.Id];
                var entityDictionaryField = entityDictionaries[entityHolder.Item1.Id];

                // create new Entity
                var primaryDbColumn = entityHolder.Item2[0];
                var primaryColumn = entityHolder.Item2[0].Item1;
                var primaryKeyLocal = _moveNextMethodIL.DeclareLocal(primaryColumn.PropertyInfo.PropertyType);

                var endOfIfLabel = _moveNextMethodIL.DefineLabel();

                // Check if lastEntity is the same as the current one
                if (entityHolder.Item1.RequiresChangedLocal)
                {
                    afterEntityGenerationIfBody = _moveNextMethodIL.DefineLabel();
                }

                if (entityHolder.Item1.RequiresDBNullCheck)
                {
                    // Check if column is null
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);

                    if (primaryDbColumn.Item2 <= sbyte.MaxValue)
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)primaryDbColumn.Item2);
                    else
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4, primaryDbColumn.Item2);

                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetMethod("IsDBNull"));
                    _moveNextMethodIL.Emit(OpCodes.Brtrue, endOfIfLabel);
                }

                // Assign the primary key to the local variable
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);

                if (primaryDbColumn.Item2 <= sbyte.MaxValue)
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)primaryDbColumn.Item2);
                else
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, primaryDbColumn.Item2);

                WriteColumnMaterializer(_moveNextMethodIL, primaryColumn);
                _moveNextMethodIL.Emit(OpCodes.Stloc_S, primaryKeyLocal);

                // Check if lastEntity is null
                var entityGenerationIfBody = _moveNextMethodIL.DefineLabel();

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, lastEntityField);
                _moveNextMethodIL.Emit(OpCodes.Brfalse, entityGenerationIfBody);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, lastEntityField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, primaryColumn.PropertyInfo.GetGetMethod());
                _moveNextMethodIL.Emit(OpCodes.Ldloc_S, primaryKeyLocal);
                WriteInEqualityComparer(primaryColumn.PropertyInfo.PropertyType, endOfIfLabel);

                // Check if the dictionary holds a instance to the current primary key
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityDictionaryField);
                _moveNextMethodIL.Emit(OpCodes.Ldloc_S, primaryKeyLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldflda, lastEntityField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityDictionaryField.FieldType.GetMethod("TryGetValue"));
                _moveNextMethodIL.Emit(OpCodes.Brtrue, entityHolder.Item1.RequiresChangedLocal ? afterEntityGenerationIfBody : endOfIfLabel);

                _moveNextMethodIL.MarkLabel(entityGenerationIfBody);

                // Instantiate the entity
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);

                CreateEntity(entity, entityHolder.Item2, changeTracking && entity.ProxyEntityType is { }, primaryKeyLocal);

                for (int i = entityHolder.Item1.InitializeNavigations.Count - 1; i >= 0; i--)
                {
                    var initializeNavigation = entityHolder.Item1.InitializeNavigations[i];

                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Newobj, typeof(List<>).MakeGenericType(new[] { initializeNavigation.LeftNavigationProperty.PropertyType.GetGenericArguments()[0] }).GetConstructor(Type.EmptyTypes));
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, initializeNavigation.LeftNavigationProperty.GetSetMethod());
                }

                _moveNextMethodIL.Emit(OpCodes.Stfld, lastEntityField);

                // return result

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityDictionaryField);
                _moveNextMethodIL.Emit(OpCodes.Ldloc_S, primaryKeyLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, lastEntityField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityDictionaryField.FieldType.GetMethod("Add"));

                if (entityHolder.Item1.RequiresChangedLocal)
                {
                    if (entity.Relations is not null)
                    {
                        var afterElseLabel = _moveNextMethodIL.DefineLabel();

                        for (int relationIndex = 0; relationIndex < entity.Relations.Count; relationIndex++)
                        {
                            var relation = entity.Relations[relationIndex];

                            if (relation.RelationType != RelationType.OneToMany ||
                                !relationDictionaries.TryGetValue(relation.RelationId, out var relationDictionaryField))
                                continue;

                            var lastRelationMapField = lastRelationMaps[relation.RelationId];

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Newobj, lastRelationMapField.FieldType.GetConstructor(Type.EmptyTypes));
                            _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, relationDictionaryField);
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relationDictionaryField.FieldType.GetMethod("Add"));
                        }

                        _moveNextMethodIL.Emit(OpCodes.Br, afterElseLabel);

                        _moveNextMethodIL.MarkLabel(afterEntityGenerationIfBody);

                        for (int relationIndex = 0; relationIndex < entity.Relations.Count; relationIndex++)
                        {
                            var relation = entity.Relations[relationIndex];

                            if (relation.RelationType != RelationType.OneToMany ||
                                !lastRelationMaps.TryGetValue(relation.RelationId, out var lastRelationMap))
                                continue;

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldnull);
                            _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMap);
                        }

                        _moveNextMethodIL.MarkLabel(afterElseLabel);
                    }
                    else
                    {
                        _moveNextMethodIL.MarkLabel(afterEntityGenerationIfBody);
                    }

                    var hasChangedLocal = _moveNextMethodIL.DeclareLocal(typeof(bool));
                    changedLocals.Add(entityHolder.Item1.Id, hasChangedLocal);

                    // set entityChanged to true
                    var afterChangedLocalToFalseLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    _moveNextMethodIL.Emit(OpCodes.Stloc_S, hasChangedLocal);
                    _moveNextMethodIL.Emit(OpCodes.Br, afterChangedLocalToFalseLabel);

                    _moveNextMethodIL.MarkLabel(endOfIfLabel);

                    // set HasChanged to false
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Stloc_S, hasChangedLocal);

                    _moveNextMethodIL.MarkLabel(afterChangedLocalToFalseLabel);
                }
                else
                {
                    _moveNextMethodIL.MarkLabel(endOfIfLabel);
                }

                // Set entity dictionary to null
                setNullGhostIL.Emit(OpCodes.Ldarg_0);
                setNullGhostIL.Emit(OpCodes.Ldnull);
                setNullGhostIL.Emit(OpCodes.Stfld, entityDictionaryField);

                // Set lastEntity to null
                setNullGhostIL.Emit(OpCodes.Ldarg_0);
                setNullGhostIL.Emit(OpCodes.Ldnull);
                setNullGhostIL.Emit(OpCodes.Stfld, lastEntityField);
            }

            if (primaryEntityHolder.Item1.HasRelations)
            {
                // Check if first row
                var afterAllLateAssignmentLabel = _moveNextMethodIL.DefineLabel();

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, isFirstRowField);
                _moveNextMethodIL.Emit(OpCodes.Brfalse, afterAllLateAssignmentLabel);

                // Set is first row to false
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Stfld, isFirstRowField);

                if (primaryEntityHolder.Item1.ForeignAssignedRelations.Count > 0)
                {
                    for (int i = primaryEntityHolder.Item1.ForeignAssignedRelations.Count - 1; i >= 0; i--)
                    {
                        var assigningRelation = primaryEntityHolder.Item1.ForeignAssignedRelations[i];

                        var lastRightEntityField = entityLastTypes[assigningRelation.Item2.Id];
                        var hasRightEntityChangedLocal = changedLocals[assigningRelation.Item2.Id];

                        var relation = assigningRelation.Item1;

                        var lastRelationMapField = lastRelationMaps[relation.RelationId];

                        var afterLateAssignmentLabel = _moveNextMethodIL.DefineLabel();

                        _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasRightEntityChangedLocal);
                        _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, lastRelationMapField.FieldType.GetMethod("Add"));
                        _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(ICollection<>).MakeGenericType(relation.RightNavigationProperty.PropertyType.GetGenericArguments()[0]).GetMethod("Add"));

                        _moveNextMethodIL.MarkLabel(afterLateAssignmentLabel);
                    }
                }

                if (primaryEntityHolder.Item1.SelfAssignedRelations.Count > 0)
                {
                    for (int i = primaryEntityHolder.Item1.SelfAssignedRelations.Count - 1; i >= 0; i--)
                    {
                        var assigningRelation = primaryEntityHolder.Item1.SelfAssignedRelations[i];

                        var lastRightEntity = entityLastTypes[assigningRelation.Item2.Id];

                        var relation = assigningRelation.Item1;

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntity);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetSetMethod());
                    }
                }

                _moveNextMethodIL.MarkLabel(afterAllLateAssignmentLabel);
            }

            for (int entityIndex = 1; entityIndex < entities.Count; entityIndex++)
            {
                var entityHolder = entities[entityIndex].Item1;

                if (!entityHolder.HasRelations)
                    continue;

                // Check if entityChanged
                var hasLeftEntityChangedLocal = changedLocals[entityHolder.Id];

                var lastLeftEntity = entityLastTypes[entityHolder.Id];

                if (entityHolder.ForeignAssignedRelations.Count > 0)
                {
                    for (int i = entityHolder.ForeignAssignedRelations.Count - 1; i >= 0; i--)
                    {
                        var assigningRelation = entityHolder.ForeignAssignedRelations[i];

                        var relation = assigningRelation.Item1;
                        var lastRightEntityField = entityLastTypes[assigningRelation.Item2.Id];

                        var relationDictionaryField = relationDictionaries[relation.RelationId];
                        var lastRelationMapField = lastRelationMaps[relation.RelationId];

                        if (assigningRelation.Item2.Id == primaryEntityHolder.Item1.Id || entityHolder.RequiresDBNullCheck)
                        {
                            var afterLateAssignmentLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasLeftEntityChangedLocal);
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                            var afterIfLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterIfLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, relationDictionaryField);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relationDictionaryField.FieldType.GetMethod("get_Item"));
                            _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                            _moveNextMethodIL.MarkLabel(afterIfLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, lastRelationMapField.FieldType.GetMethod("Add"));
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(ICollection<>).MakeGenericType(relation.RightNavigationProperty.PropertyType.GetGenericArguments()[0]).GetMethod("Add"));
                            _moveNextMethodIL.MarkLabel(afterLateAssignmentLabel);
                        }
                        else
                        {
                            var hasRightEntityChangedLocal = changedLocals[assigningRelation.Item2.Id];

                            var afterLateAssignmentLabel = _moveNextMethodIL.DefineLabel();
                            var oneToManyAssignmentLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasLeftEntityChangedLocal);
                            _moveNextMethodIL.Emit(OpCodes.Brtrue, oneToManyAssignmentLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasRightEntityChangedLocal);
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                            _moveNextMethodIL.MarkLabel(oneToManyAssignmentLabel);

                            var afterIfLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterIfLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, relationDictionaryField);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relationDictionaryField.FieldType.GetMethod("get_Item"));
                            _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                            _moveNextMethodIL.MarkLabel(afterIfLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, lastRelationMapField.FieldType.GetMethod("Add"));
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(ICollection<>).MakeGenericType(relation.RightNavigationProperty.PropertyType.GetGenericArguments()[0]).GetMethod("Add"));

                            _moveNextMethodIL.MarkLabel(afterLateAssignmentLabel);
                        }
                    }
                }

                if (entityHolder.SelfAssignedRelations.Count > 0)
                {
                    var afterLateAssignmentLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasLeftEntityChangedLocal);
                    _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                    for (int i = entityHolder.SelfAssignedRelations.Count - 1; i >= 0; i--)
                    {
                        var assigningRelation = entityHolder.SelfAssignedRelations[i];

                        var lastRightEntity = entityLastTypes[assigningRelation.Item2.Id];

                        var relation = assigningRelation.Item1;

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntity);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetSetMethod());
                    }

                    _moveNextMethodIL.MarkLabel(afterLateAssignmentLabel);
                }
            }

            _moveNextMethodIL.MarkLabel(loopConditionLabel);

            // Call ReadAsync(cancellationToken) on dataReader
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }));

            asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), _boolTaskAwaiterLocal, _boolTaskAwaiterField);

            _moveNextMethodIL.Emit(OpCodes.Brtrue, loopBodyLabel);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            // End of try block

            // Start of catch block
            _moveNextMethodIL.BeginCatchBlock(_defaultExceptionLocal.LocalType);

            // Set state and return exception
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _defaultExceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);

            setNullGhostIL.WriteIL(_moveNextMethodIL);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _defaultExceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            // End of catch block
            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfCatchLabel);

            // Set state and return result
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            setNullGhostIL.WriteIL(_moveNextMethodIL);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetResult"));

            // End of method
            _moveNextMethodIL.MarkLabel(endOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);
        }

        private void CreateBatchNoRelationMaterializer(List<(EntityColumn, int)> dbColumns, bool changeTracking)
        {
            var resultField = _stateMachineTypeBuilder.DefineField("_result", _returnType, FieldAttributes.Private);
            var resultLocal = _moveNextMethodIL.DeclareLocal(_returnType);

            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var endOfCatchLabel = _moveNextMethodIL.DefineLabel();
            var loopBodyLabel = _moveNextMethodIL.DefineLabel();
            var loopConditionLabel = _moveNextMethodIL.DefineLabel();

            // Assign the state field to the state local
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _stateLocal);

            // Start try block
            _moveNextMethodIL.BeginExceptionBlock();

            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _stateLocal);

            var switchBuilder = new ILSwitchBuilder(_moveNextMethodIL, 1);

            var asyncGenerator = new ILAsyncGenerator(_moveNextMethodIL, switchBuilder, _methodBuilderField, _stateField, _stateLocal, endOfMethodLabel, _stateMachineTypeBuilder);

            _moveNextMethodIL.Emit(OpCodes.Brfalse, switchBuilder.GetLabels()[0]);

            // Create result field instance

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_4);
            _moveNextMethodIL.Emit(OpCodes.Newobj, resultLocal.LocalType.GetConstructor(new[] { typeof(int) }));
            _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);

            // if no rows return
            var afterNoRowsIfBody = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetProperty("HasRows").GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterNoRowsIfBody);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
            _moveNextMethodIL.Emit(OpCodes.Stloc, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            _moveNextMethodIL.MarkLabel(afterNoRowsIfBody);

            _moveNextMethodIL.Emit(OpCodes.Br, loopConditionLabel);

            _moveNextMethodIL.MarkLabel(loopBodyLabel);

            // create new Entity

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);

            CreateEntity(_rootEntity, dbColumns, changeTracking);

            // return result

            _moveNextMethodIL.Emit(OpCodes.Callvirt, resultField.FieldType.GetMethod("Add"));

            _moveNextMethodIL.MarkLabel(loopConditionLabel);

            // Call ReadAsync(cancellationToken) on dataReader
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }));

            asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), _boolTaskAwaiterLocal, _boolTaskAwaiterField);

            _moveNextMethodIL.Emit(OpCodes.Brtrue, loopBodyLabel);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            // End of try block

            // Start of catch block
            _moveNextMethodIL.BeginCatchBlock(_defaultExceptionLocal.LocalType);

            // Set state and return exception
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _defaultExceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _defaultExceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            // End of catch block
            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfCatchLabel);

            // Set state and return result
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetResult"));

            // End of method
            _moveNextMethodIL.MarkLabel(endOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);
        }

        private void CreateBatchRelationMaterializer(List<(QueryEntityHolder, List<(EntityColumn, int)>)> entities, bool changeTracking)
        {
            var resultField = _stateMachineTypeBuilder.DefineField("_result", _returnType, FieldAttributes.Private);
            var resultLocal = _moveNextMethodIL.DeclareLocal(_returnType);

            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var endOfCatchLabel = _moveNextMethodIL.DefineLabel();
            var loopBodyLabel = _moveNextMethodIL.DefineLabel();
            var loopConditionLabel = _moveNextMethodIL.DefineLabel();

            // Assign the state field to the state local
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _stateLocal);

            // Start try block
            _moveNextMethodIL.BeginExceptionBlock();

            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _stateLocal);

            var switchBuilder = new ILSwitchBuilder(_moveNextMethodIL, 1);

            var asyncGenerator = new ILAsyncGenerator(_moveNextMethodIL, switchBuilder, _methodBuilderField, _stateField, _stateLocal, endOfMethodLabel, _stateMachineTypeBuilder);

            _moveNextMethodIL.Emit(OpCodes.Brfalse, switchBuilder.GetLabels()[0]);

            // Create result field instance

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_4);
            _moveNextMethodIL.Emit(OpCodes.Newobj, resultLocal.LocalType.GetConstructor(new[] { typeof(int) }));
            _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);

            // if no rows return
            var afterNoRowsIfBody = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetProperty("HasRows").GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterNoRowsIfBody);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
            _moveNextMethodIL.Emit(OpCodes.Stloc, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            _moveNextMethodIL.MarkLabel(afterNoRowsIfBody);

            var entityDictionaries = new Dictionary<int, FieldBuilder>(entities.Count);
            var entityLastTypes = new Dictionary<int, FieldBuilder>(entities.Count);

            var relationDictionaries = new Dictionary<uint, FieldBuilder>(entities.Count);
            var lastRelationMaps = new Dictionary<uint, FieldBuilder>(entities.Count);

            var dictionaryType = typeof(Dictionary<,>);
            var hashSetType = typeof(HashSet<>);

            for (int entityIndex = entities.Count - 1; entityIndex >= 0; entityIndex--)
            {
                var entityHolder = entities[entityIndex];
                var entity = entityHolder.Item1.Entity;

                // Add dictionary field
                var entityDictionaryField = _stateMachineTypeBuilder.DefineField("_" + entity.EntityName + entityHolder.Item1.Id, dictionaryType.MakeGenericType(entity.GetPrimaryColumn().PropertyInfo.PropertyType, entity.EntityType), FieldAttributes.Private);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Newobj, entityDictionaryField.FieldType.GetConstructor(Type.EmptyTypes));
                _moveNextMethodIL.Emit(OpCodes.Stfld, entityDictionaryField);

                entityDictionaries.Add(entityHolder.Item1.Id, entityDictionaryField);

                // Add lastEntity field

                var lastEntityField = _stateMachineTypeBuilder.DefineField("_last" + entity.EntityName + entityHolder.Item1.Id, entity.EntityType, FieldAttributes.Private);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, lastEntityField);

                entityLastTypes.Add(entityHolder.Item1.Id, lastEntityField);

                for (int relationIndex = 0; relationIndex < entityHolder.Item1.ForeignAssignedRelations.Count; relationIndex++)
                {
                    var relation = entityHolder.Item1.ForeignAssignedRelations[relationIndex].Item1;

                    if (relation.RelationType != RelationType.ManyToOne)
                        continue;

                    // Add relationDictionary field

                    var relationDictionaryName = entity.EntityName + "_Relation" + entityHolder.Item1.Id + "_" + relation.RelationId;

                    var relationMap = hashSetType.MakeGenericType(relation.RightEntity.GetPrimaryColumn().PropertyInfo.PropertyType);

                    var relationDictionaryField = _stateMachineTypeBuilder.DefineField("_" + relationDictionaryName, dictionaryType.MakeGenericType(entity.GetPrimaryColumn().PropertyInfo.PropertyType, relationMap), FieldAttributes.Private);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Newobj, relationDictionaryField.FieldType.GetConstructor(Type.EmptyTypes));
                    _moveNextMethodIL.Emit(OpCodes.Stfld, relationDictionaryField);

                    relationDictionaries.Add(relation.RelationId, relationDictionaryField);

                    // Add lastRelationMap field

                    var lastRelationMapField = _stateMachineTypeBuilder.DefineField("_last" + relationDictionaryName + "_Map", relationMap, FieldAttributes.Private);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldnull);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                    lastRelationMaps.Add(relation.RelationId, lastRelationMapField);
                }
            }

            _moveNextMethodIL.Emit(OpCodes.Br, loopConditionLabel);

            _moveNextMethodIL.MarkLabel(loopBodyLabel);
            var setNullGhostIL = new ILGhostGenerator();

            // Set result field to null
            setNullGhostIL.Emit(OpCodes.Ldarg_0);
            setNullGhostIL.Emit(OpCodes.Ldnull);
            setNullGhostIL.Emit(OpCodes.Stfld, resultField);

            var changedLocals = new Dictionary<int, LocalBuilder>(entities.Count);

            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++)
            {
                var entityHolder = entities[entityIndex];

                var entity = entityHolder.Item1.Entity;
                var lastEntityField = entityLastTypes[entityHolder.Item1.Id];
                var entityDictionaryField = entityDictionaries[entityHolder.Item1.Id];

                // create new Entity
                var primaryDbColumn = entityHolder.Item2[0];
                var primaryColumn = entityHolder.Item2[0].Item1;
                var primaryKeyLocal = _moveNextMethodIL.DeclareLocal(primaryColumn.PropertyInfo.PropertyType);

                // Check if lastEntity is the same as the current one
                var endOfIfLabel = _moveNextMethodIL.DefineLabel();

                Label? afterEntityGenerationIfBody = default;

                if (entityHolder.Item1.RequiresChangedLocal)
                {
                    afterEntityGenerationIfBody = _moveNextMethodIL.DefineLabel();
                }

                if (entityHolder.Item1.RequiresDBNullCheck)
                {
                    // Check if column is null
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);

                    if (primaryDbColumn.Item2 <= sbyte.MaxValue)
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)primaryDbColumn.Item2);
                    else
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4, primaryDbColumn.Item2);

                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetMethod("IsDBNull"));
                    _moveNextMethodIL.Emit(OpCodes.Brtrue, endOfIfLabel);
                }

                // Assign the primary key to the local variable
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);

                if (primaryDbColumn.Item2 <= sbyte.MaxValue)
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)primaryDbColumn.Item2);
                else
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, primaryDbColumn.Item2);

                WriteColumnMaterializer(_moveNextMethodIL, primaryColumn);
                _moveNextMethodIL.Emit(OpCodes.Stloc_S, primaryKeyLocal);

                // Check if lastEntity is null
                var entityGenerationIfBody = _moveNextMethodIL.DefineLabel();

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, lastEntityField);
                _moveNextMethodIL.Emit(OpCodes.Brfalse, entityGenerationIfBody);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, lastEntityField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, primaryColumn.PropertyInfo.GetGetMethod());
                _moveNextMethodIL.Emit(OpCodes.Ldloc_S, primaryKeyLocal);
                WriteInEqualityComparer(primaryColumn.PropertyInfo.PropertyType, endOfIfLabel);

                // Check if the dictionary holds a instance to the current primary key
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityDictionaryField);
                _moveNextMethodIL.Emit(OpCodes.Ldloc_S, primaryKeyLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldflda, lastEntityField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityDictionaryField.FieldType.GetMethod("TryGetValue"));
                _moveNextMethodIL.Emit(OpCodes.Brtrue, entityHolder.Item1.RequiresChangedLocal ? afterEntityGenerationIfBody.Value : endOfIfLabel);

                _moveNextMethodIL.MarkLabel(entityGenerationIfBody);

                // Instantiate the entity
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);

                CreateEntity(entity, entityHolder.Item2, changeTracking && entity.ProxyEntityType is { }, primaryKeyLocal);

                for (int i = entityHolder.Item1.InitializeNavigations.Count - 1; i >= 0; i--)
                {
                    var initializeNavigation = entityHolder.Item1.InitializeNavigations[i];

                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Newobj, typeof(List<>).MakeGenericType(new[] { initializeNavigation.LeftNavigationProperty.PropertyType.GetGenericArguments()[0] }).GetConstructor(Type.EmptyTypes));
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, initializeNavigation.LeftNavigationProperty.GetSetMethod());
                }

                _moveNextMethodIL.Emit(OpCodes.Stfld, lastEntityField);

                // return result
                if (entityIndex == 0)
                {
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, lastEntityField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, resultField.FieldType.GetMethod("Add"));
                }

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityDictionaryField);
                _moveNextMethodIL.Emit(OpCodes.Ldloc_S, primaryKeyLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, lastEntityField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityDictionaryField.FieldType.GetMethod("Add"));

                if (entityHolder.Item1.RequiresChangedLocal)
                {
                    if (entity.Relations is not null)
                    {
                        var afterElseLabel = _moveNextMethodIL.DefineLabel();

                        for (int relationIndex = 0; relationIndex < entity.Relations.Count; relationIndex++)
                        {
                            var relation = entity.Relations[relationIndex];

                            if (relation.RelationType != RelationType.OneToMany ||
                                !relationDictionaries.TryGetValue(relation.RelationId, out var relationDictionaryField))
                                continue;

                            var lastRelationMapField = lastRelationMaps[relation.RelationId];

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Newobj, lastRelationMapField.FieldType.GetConstructor(Type.EmptyTypes));
                            _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, relationDictionaryField);
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relationDictionaryField.FieldType.GetMethod("Add"));
                        }

                        _moveNextMethodIL.Emit(OpCodes.Br, afterElseLabel);

                        _moveNextMethodIL.MarkLabel(afterEntityGenerationIfBody.Value);

                        for (int relationIndex = 0; relationIndex < entity.Relations.Count; relationIndex++)
                        {
                            var relation = entity.Relations[relationIndex];

                            if (relation.RelationType != RelationType.OneToMany ||
                                !lastRelationMaps.TryGetValue(relation.RelationId, out var lastRelationMap))
                                continue;

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldnull);
                            _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMap);
                        }

                        _moveNextMethodIL.MarkLabel(afterElseLabel);
                    }
                    else
                    {
                        _moveNextMethodIL.MarkLabel(afterEntityGenerationIfBody.Value);
                    }

                    var hasChangedLocal = _moveNextMethodIL.DeclareLocal(typeof(bool));
                    changedLocals.Add(entityHolder.Item1.Id, hasChangedLocal);

                    // set entityChanged to true
                    var afterChangedLocalToFalseLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    _moveNextMethodIL.Emit(OpCodes.Stloc_S, hasChangedLocal);
                    _moveNextMethodIL.Emit(OpCodes.Br, afterChangedLocalToFalseLabel);

                    _moveNextMethodIL.MarkLabel(endOfIfLabel);

                    // set HasChanged to false
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Stloc_S, hasChangedLocal);

                    _moveNextMethodIL.MarkLabel(afterChangedLocalToFalseLabel);
                }
                else
                {
                    _moveNextMethodIL.MarkLabel(endOfIfLabel);
                }

                // Set entity dictionary to null
                setNullGhostIL.Emit(OpCodes.Ldarg_0);
                setNullGhostIL.Emit(OpCodes.Ldnull);
                setNullGhostIL.Emit(OpCodes.Stfld, entityDictionaryField);

                // Set lastEntity to null
                setNullGhostIL.Emit(OpCodes.Ldarg_0);
                setNullGhostIL.Emit(OpCodes.Ldnull);
                setNullGhostIL.Emit(OpCodes.Stfld, lastEntityField);
            }

            for (int entityIndex = entities.Count - 1; entityIndex >= 0; entityIndex--)
            {
                var entityHolder = entities[entityIndex].Item1;

                if (!entityHolder.HasRelations)
                    continue;

                // Check if entityChanged
                var hasLeftEntityChangedLocal = changedLocals[entityHolder.Id];
                var lastLeftEntity = entityLastTypes[entityHolder.Id];

                if (entityHolder.ForeignAssignedRelations.Count > 0)
                {
                    for (int i = entityHolder.ForeignAssignedRelations.Count - 1; i >= 0; i--)
                    {
                        var assigningRelation = entityHolder.ForeignAssignedRelations[i];

                        var relation = assigningRelation.Item1;
                        var lastRightEntityField = entityLastTypes[assigningRelation.Item2.Id];

                        var relationDictionaryField = relationDictionaries[relation.RelationId];
                        var lastRelationMapField = lastRelationMaps[relation.RelationId];

                        if (entityHolder.RequiresDBNullCheck)
                        {
                            var afterLateAssignmentLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasLeftEntityChangedLocal);
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                            var afterIfLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterIfLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, relationDictionaryField);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relationDictionaryField.FieldType.GetMethod("get_Item"));
                            _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                            _moveNextMethodIL.MarkLabel(afterIfLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, lastRelationMapField.FieldType.GetMethod("Add"));
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);

                            _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(ICollection<>).MakeGenericType(relation.RightNavigationProperty.PropertyType.GetGenericArguments()[0]).GetMethod("Add"));

                            _moveNextMethodIL.MarkLabel(afterLateAssignmentLabel);
                        }
                        else
                        {
                            var hasRightEntityChangedLocal = changedLocals[assigningRelation.Item2.Id];

                            var afterLateAssignmentLabel = _moveNextMethodIL.DefineLabel();
                            var oneToManyAssignmentLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasLeftEntityChangedLocal);
                            _moveNextMethodIL.Emit(OpCodes.Brtrue, oneToManyAssignmentLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasRightEntityChangedLocal);
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                            _moveNextMethodIL.MarkLabel(oneToManyAssignmentLabel);

                            var afterIfLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterIfLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, relationDictionaryField);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relationDictionaryField.FieldType.GetMethod("get_Item"));
                            _moveNextMethodIL.Emit(OpCodes.Stfld, lastRelationMapField);

                            _moveNextMethodIL.MarkLabel(afterIfLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRelationMapField);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, lastRelationMapField.FieldType.GetMethod("Add"));
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntityField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(ICollection<>).MakeGenericType(relation.RightNavigationProperty.PropertyType.GetGenericArguments()[0]).GetMethod("Add"));

                            _moveNextMethodIL.MarkLabel(afterLateAssignmentLabel);
                        }
                    }
                }

                if (entityHolder.SelfAssignedRelations.Count > 0)
                {
                    var afterLateAssignmentLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldloc_S, hasLeftEntityChangedLocal);
                    _moveNextMethodIL.Emit(OpCodes.Brfalse, afterLateAssignmentLabel);

                    for (int i = entityHolder.SelfAssignedRelations.Count - 1; i >= 0; i--)
                    {
                        var assigningRelation = entityHolder.SelfAssignedRelations[i];

                        var lastRightEntity = entityLastTypes[assigningRelation.Item2.Id];

                        var relation = assigningRelation.Item1;

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, lastLeftEntity);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, lastRightEntity);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetSetMethod());
                    }

                    _moveNextMethodIL.MarkLabel(afterLateAssignmentLabel);
                }
            }

            _moveNextMethodIL.MarkLabel(loopConditionLabel);

            // Call ReadAsync(cancellationToken) on dataReader
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }));

            asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), _boolTaskAwaiterLocal, _boolTaskAwaiterField);

            _moveNextMethodIL.Emit(OpCodes.Brtrue, loopBodyLabel);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfCatchLabel);

            // End of try block

            // Start of catch block
            _moveNextMethodIL.BeginCatchBlock(_defaultExceptionLocal.LocalType);

            // Set state and return exception
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _defaultExceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);

            setNullGhostIL.WriteIL(_moveNextMethodIL);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _defaultExceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            // End of catch block
            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfCatchLabel);

            // Set state and return result
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            setNullGhostIL.WriteIL(_moveNextMethodIL);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, resultLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetResult"));

            // End of method
            _moveNextMethodIL.MarkLabel(endOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);
        }

        private void CreateEntity(Entity entity, List<(EntityColumn, int)> dbColumns, bool changeTracking, LocalBuilder? primaryKeyLocal = null)
        {
            LocalBuilder? changeTrackerLocal = default;

            if (changeTracking)
            {
                var changeTrackerType = typeof(ChangeTracker<>).MakeGenericType(entity.EntityType);

                changeTrackerLocal = _moveNextMethodIL.DeclareLocal(changeTrackerType);

                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, entity.GetColumnCount());
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Newobj, changeTrackerType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(bool) }, null));
                _moveNextMethodIL.Emit(OpCodes.Stloc_S, changeTrackerLocal);

                _moveNextMethodIL.Emit(OpCodes.Ldloc, changeTrackerLocal);
                _moveNextMethodIL.Emit(OpCodes.Newobj, entity.ProxyEntityType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { changeTrackerType }, null));
            }
            else
            {
                _moveNextMethodIL.Emit(OpCodes.Newobj, entity.EntityType.GetConstructor(Type.EmptyTypes));
            }

            var columnIteratorIndex = 0;

            if (primaryKeyLocal is { })
            {
                columnIteratorIndex = entity.GetRegularColumnOffset();

                (var column, _) = dbColumns[0];

                _moveNextMethodIL.Emit(OpCodes.Dup);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
            }

            for (; columnIteratorIndex < dbColumns.Count; columnIteratorIndex++)
            {
                // Assign the property the value from the reader

                (var column, var columnIndex) = dbColumns[columnIteratorIndex];

                _moveNextMethodIL.Emit(OpCodes.Dup);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);

                if (columnIndex <= sbyte.MaxValue)
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)columnIndex);
                else
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, columnIndex);

                WriteColumnMaterializer(_moveNextMethodIL, column);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
            }

            if (changeTracking)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldloc, changeTrackerLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, changeTrackerLocal.LocalType.GetProperty("TrackChanges", BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true));
            }
        }

        private void WriteInEqualityComparer(Type type, Label branchTo)
        {
            var customInEqualityMethod = type.GetMethod("op_Inequality");

            if (customInEqualityMethod is { })
            {
                _moveNextMethodIL.Emit(OpCodes.Call, customInEqualityMethod);
                _moveNextMethodIL.Emit(OpCodes.Brfalse, branchTo);
            }
            else
            {
                _moveNextMethodIL.Emit(OpCodes.Beq, branchTo);
            }
        }

        private void WriteColumnMaterializer(ILGenerator iLGenerator, EntityColumn column)
        {
            if (column.IsNullableReferenceType)
            {
                var valueRetriever = typeof(NpgsqlDataReaderExtensions).GetMethod("GetValueOrDefault", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(column.PropertyInfo.PropertyType);

                iLGenerator.Emit(OpCodes.Call, valueRetriever);
            }
            else
            {
                var valueRetriever = _dataReaderType.GetMethod("GetFieldValue", BindingFlags.Instance | BindingFlags.Public);

                if (column is not IPostgreEnumEntityColumn)
                {
                    var underlyingType = Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType);

                    if (underlyingType is { })
                    {
                        if (underlyingType.IsEnum)
                        {
                            var underlyingNumericalType = Enum.GetUnderlyingType(underlyingType);
                            var nullableUnderlyingEnumType = typeof(Nullable<>).MakeGenericType(underlyingNumericalType);

                            var nullableUnderylyingEnumLocal = iLGenerator.DeclareLocal(nullableUnderlyingEnumType);
                            var enumLocal = iLGenerator.DeclareLocal(column.PropertyInfo.PropertyType);

                            var afterHasNoValueLabel = iLGenerator.DefineLabel();
                            var assignLabel = iLGenerator.DefineLabel();

                            valueRetriever = valueRetriever.MakeGenericMethod(nullableUnderlyingEnumType);

                            iLGenerator.Emit(OpCodes.Callvirt, valueRetriever);
                            iLGenerator.Emit(OpCodes.Stloc_S, nullableUnderylyingEnumLocal);

                            iLGenerator.Emit(OpCodes.Ldloca_S, nullableUnderylyingEnumLocal);
                            iLGenerator.Emit(OpCodes.Call, nullableUnderlyingEnumType.GetProperty("HasValue").GetGetMethod());
                            iLGenerator.Emit(OpCodes.Brtrue, afterHasNoValueLabel);

                            iLGenerator.Emit(OpCodes.Ldloca_S, enumLocal);
                            iLGenerator.Emit(OpCodes.Initobj, column.PropertyInfo.PropertyType);
                            iLGenerator.Emit(OpCodes.Ldloc_S, enumLocal);
                            iLGenerator.Emit(OpCodes.Br, assignLabel);

                            iLGenerator.MarkLabel(afterHasNoValueLabel);

                            iLGenerator.Emit(OpCodes.Ldloca_S, nullableUnderylyingEnumLocal);
                            iLGenerator.Emit(OpCodes.Call, nullableUnderlyingEnumType.GetProperty("Value").GetGetMethod());

                            iLGenerator.Emit(OpCodes.Newobj, column.PropertyInfo.PropertyType.GetConstructor(new[] { underlyingType }));

                            iLGenerator.MarkLabel(assignLabel);

                            return;
                        }
                        else if (underlyingType == typeof(ulong))
                        {
                            var nullableLongType = typeof(long?);

                            var nullableLongLocal = iLGenerator.DeclareLocal(nullableLongType);
                            var nullableUlongLocal = iLGenerator.DeclareLocal(column.PropertyInfo.PropertyType);

                            var afterHasNoValueLabel = iLGenerator.DefineLabel();
                            var assignLabel = iLGenerator.DefineLabel();

                            valueRetriever = valueRetriever.MakeGenericMethod(nullableLongType);

                            iLGenerator.Emit(OpCodes.Callvirt, valueRetriever);
                            iLGenerator.Emit(OpCodes.Stloc_S, nullableLongLocal);

                            iLGenerator.Emit(OpCodes.Ldloca_S, nullableLongLocal);
                            iLGenerator.Emit(OpCodes.Call, nullableLongType.GetProperty("HasValue").GetGetMethod());
                            iLGenerator.Emit(OpCodes.Brtrue, afterHasNoValueLabel);

                            iLGenerator.Emit(OpCodes.Ldloca_S, nullableUlongLocal);
                            iLGenerator.Emit(OpCodes.Initobj, column.PropertyInfo.PropertyType);
                            iLGenerator.Emit(OpCodes.Ldloc_S, nullableUlongLocal);
                            iLGenerator.Emit(OpCodes.Br, assignLabel);

                            iLGenerator.MarkLabel(afterHasNoValueLabel);

                            iLGenerator.Emit(OpCodes.Ldloca_S, nullableLongLocal);
                            iLGenerator.Emit(OpCodes.Call, nullableLongType.GetProperty("Value").GetGetMethod());

                            iLGenerator.Emit(OpCodes.Ldc_I8, long.MinValue);
                            iLGenerator.Emit(OpCodes.Sub);

                            iLGenerator.Emit(OpCodes.Newobj, column.PropertyInfo.PropertyType.GetConstructor(new[] { underlyingType }));

                            iLGenerator.MarkLabel(assignLabel);

                            return;
                        }
                    }
                    else
                    {
                        if (column.PropertyInfo.PropertyType.IsEnum)
                        {
                            valueRetriever = valueRetriever.MakeGenericMethod(Enum.GetUnderlyingType(column.PropertyInfo.PropertyType));

                            iLGenerator.Emit(OpCodes.Callvirt, valueRetriever);

                            return;
                        }
                    }
                }

                var type = column.PropertyInfo.PropertyType;

                var isUlong = type == typeof(ulong);

                if (isUlong)
                {
                    type = typeof(long);
                }

                valueRetriever = valueRetriever.MakeGenericMethod(type);

                iLGenerator.Emit(OpCodes.Callvirt, valueRetriever);

                if (isUlong)
                {
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I8, long.MinValue);
                    _moveNextMethodIL.Emit(OpCodes.Sub);
                }
            }
        }
    }
}