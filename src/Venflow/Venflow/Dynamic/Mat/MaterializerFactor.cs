using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Venflow.Commands;
using Venflow.Dynamic.IL;
using Venflow.Dynamic.Proxies;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Mat
{
    internal class QueryEntityHolder
    {
        internal int Id { get; }
        internal Entity Entity { get; }
        internal List<EntityRelation> AssigningRelations { get; }
        internal List<EntityRelation> AssignedRelations { get; }

        internal QueryEntityHolder(Entity entity, int id)
        {
            AssigningRelations = new List<EntityRelation>();
            AssignedRelations = new List<EntityRelation>();

            Entity = entity;
            Id = id;
        }
    }

    internal class MaterializerSourceCompiler
    {
        private int _queryEntityHolderIndex;
        private readonly LinkedList<QueryEntityHolder> _entities;
        private readonly JoinBuilderValues _joinBuilderValues;

        internal MaterializerSourceCompiler(JoinBuilderValues joinBuilderValues)
        {
            _entities = new LinkedList<QueryEntityHolder>();

            _joinBuilderValues = joinBuilderValues;
        }

        internal QueryEntityHolder[] GenerateSortedEntities()
        {
            var entities = new QueryEntityHolder[_entities.Count];

            var index = 0;

            for (var entry = _entities.First; entry is { }; entry = entry.Next)
            {
                entities[index++] = entry.Value;
            }

            return entities;
        }

        internal void Compile()
        {
            _entities.AddFirst(new QueryEntityHolder(_joinBuilderValues.Root, _queryEntityHolderIndex++));

            for (int i = 0; i < _joinBuilderValues.FullPath.Count; i++)
            {
                BaseCompile(_joinBuilderValues.FullPath[i]);
            }
        }

        private void BaseCompile(JoinPath joinPath)
        {
            var leftQueryHolder = new QueryEntityHolder(joinPath.JoinOptions.Join.RightEntity, _queryEntityHolderIndex++);

            _entities.AddLast(leftQueryHolder);

            leftQueryHolder.AssigningRelations.Add(joinPath.JoinOptions.Join);

            if (joinPath.JoinOptions.Join.RightNavigationProperty is { })
            {
                leftQueryHolder.AssignedRelations.Add(joinPath.JoinOptions.Join.Sibiling);
            }

            for (int i = 0; i < joinPath.TrailingJoinPath.Count; i++)
            {
                BaseCompile(joinPath.TrailingJoinPath[i]);
            }
        }
    }

    internal class MaterializerFactory
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

        internal MaterializerFactory(Entity rootEntity)
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


        internal Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> CreateMaterializer<TReturn>(List<(QueryEntityHolder, List<(EntityColumn, int)>)> entities, bool changeTracking) where TReturn : class
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
                    CreateSingleNoRelationMaterializer(entities[0].Item2, changeTracking);
                }
            }
            else
            {
                if (entities.Count == 1)
                {
                    CreateBatchNoRelationMaterializer(entities[0].Item2, changeTracking);
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
            var awaitUnsafeEndLabel = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _stateLocal);
            _moveNextMethodIL.Emit(OpCodes.Brfalse, awaitUnsafeEndLabel);

            // Call ReadAsync(cancellationToken) on dataReader
            ExecuteAsyncMethod(_dataReaderField, _dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }), _boolTaskAwaiterField, _boolTaskAwaiterLocal, awaitUnsafeEndLabel, endOfMethodLabel);
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

            // if state zero goto await unsafe
            var awaitUnsafeEndLabel = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, _stateLocal);
            _moveNextMethodIL.Emit(OpCodes.Brfalse, awaitUnsafeEndLabel);

            // Create result field instance

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Newobj, resultLocal.LocalType.GetConstructor(Type.EmptyTypes));
            _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);
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
            ExecuteAsyncMethod(_dataReaderField, _dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }), _boolTaskAwaiterField, _boolTaskAwaiterLocal, awaitUnsafeEndLabel, endOfMethodLabel);
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

        private void CreateEntity(Entity entity, List<(EntityColumn, int)> dbColumns, bool changeTracking)
        {
            LocalBuilder? changeTrackerLocal = default;

            if (changeTracking)
            {
                var changeTrackerType = typeof(ChangeTracker<>).MakeGenericType(_rootEntity.EntityType);

                changeTrackerLocal = _moveNextMethodIL.DeclareLocal(changeTrackerType);

                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, _rootEntity.GetColumnCount());
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Newobj, changeTrackerType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(bool) }, null));
                _moveNextMethodIL.Emit(OpCodes.Stloc, changeTrackerLocal);

                _moveNextMethodIL.Emit(OpCodes.Ldloc, changeTrackerLocal);
                _moveNextMethodIL.Emit(OpCodes.Newobj, entity.ProxyEntityType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { changeTrackerType }, null));
            }
            else
            {
                _moveNextMethodIL.Emit(OpCodes.Newobj, entity.EntityType.GetConstructor(Type.EmptyTypes));
            }

            for (int i = 0; i < dbColumns.Count; i++)
            {
                // Assign the property the value from the reader

                (var column, var columnIndex) = dbColumns[i];

                _moveNextMethodIL.Emit(OpCodes.Dup);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _dataReaderField);

                if (columnIndex <= sbyte.MaxValue)
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)columnIndex);
                else
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, columnIndex);

                GetColumnMaterializer(_moveNextMethodIL, column);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod());
            }

            if (changeTracking)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldloc, changeTrackerLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, changeTrackerLocal.LocalType.GetProperty("TrackChanges", BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true));
            }
        }

        private void ExecuteAsyncMethod(FieldBuilder instanceField, MethodInfo asyncMethod, FieldBuilder taskAwaiterField, LocalBuilder taskAwaiterLocal, Label awaitUnsafeEndLabel, Label endOfMethodLabel)
        {
            var resultLabel = _moveNextMethodIL.DefineLabel();

            // Execute async method and get awaiter
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, instanceField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, asyncMethod);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, asyncMethod.ReturnType.GetMethod("GetAwaiter"));
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, taskAwaiterLocal);

            // Check if the method completed sync
            _moveNextMethodIL.Emit(OpCodes.Ldloca_S, taskAwaiterLocal);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, taskAwaiterLocal.LocalType.GetProperty("IsCompleted").GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Brtrue, resultLabel);

            // Assign 0 to the state local and field
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
            _moveNextMethodIL.Emit(OpCodes.Dup);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _stateLocal);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);

            // Assign the task awaiter local to the field
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, taskAwaiterLocal);
            _moveNextMethodIL.Emit(OpCodes.Stfld, taskAwaiterField);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloca_S, taskAwaiterLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(taskAwaiterLocal.LocalType, _stateMachineTypeBuilder));

            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            // If TaskAwaiter is complete create task awaiter and change state
            _moveNextMethodIL.MarkLabel(awaitUnsafeEndLabel);

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, taskAwaiterField);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, taskAwaiterLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, taskAwaiterField);
            _moveNextMethodIL.Emit(OpCodes.Initobj, taskAwaiterField.FieldType);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
            _moveNextMethodIL.Emit(OpCodes.Dup);
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, _stateLocal);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);

            // Call GetResult
            _moveNextMethodIL.MarkLabel(resultLabel);
            _moveNextMethodIL.Emit(OpCodes.Ldloca_S, taskAwaiterLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, taskAwaiterLocal.LocalType.GetMethod("GetResult"));
        }

        private void GetColumnMaterializer(ILGenerator iLGenerator, EntityColumn column)
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