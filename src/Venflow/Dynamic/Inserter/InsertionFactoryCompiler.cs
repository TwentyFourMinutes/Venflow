#pragma warning disable 8602,8604
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Npgsql;
using Venflow.Dynamic.IL;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    // TODO: Add CommandBehaviour.SingleRow to batch inserts
    // TODO: Consider adding Spans
    internal class InsertionFactoryCompiler
    {
        private FieldBuilder _commandField = null!;
        private FieldBuilder _rootEntityInsertField = null!;
        private FieldBuilder _cancellationTokenField = null!;
        private FieldBuilder? _loggerField;

        private FieldBuilder _stateField = null!;
        private LocalBuilder _stateLocal = null!;
        private Type _insertType = null!;

        private FieldBuilder _methodBuilderField = null!;

        private TypeBuilder _inserterTypeBuilder = null!;
        private TypeBuilder _stateMachineTypeBuilder = null!;

        private MethodBuilder _moveNextMethod = null!;
        private ILGenerator _moveNextMethodIL = null!;

        private ObjectIDGenerator _reachableEntities = null!;
        private HashSet<uint> _reachableRelations = null!;

        private readonly Type _intType = typeof(int);
        private readonly Type _genericICollectionType = typeof(ICollection<>);

        private readonly Entity _rootEntity;

        internal InsertionFactoryCompiler(Entity rootEntity)
        {
            _rootEntity = rootEntity;
        }

        internal Delegate CreateInserter<TInsert>(EntityRelationHolder[] entities, ObjectIDGenerator reachableEntities, HashSet<uint> reachableRelations, bool shouldLog) where TInsert : class
        {
            _reachableEntities = reachableEntities;
            _reachableRelations = reachableRelations;

            _insertType = typeof(TInsert);

            var isSingleInsert = _insertType == _rootEntity.EntityType;

            if (_rootEntity.HasDbGeneratedPrimaryKey ||
                !isSingleInsert ||
                entities.Length > 1 ||
                shouldLog)
            {
                _inserterTypeBuilder = TypeFactory.GetNewInserterBuilder(_rootEntity.EntityName, TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);
                _stateMachineTypeBuilder = _inserterTypeBuilder.DefineNestedType("StateMachine", TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, typeof(ValueType), new[] { typeof(IAsyncStateMachine) });

                _moveNextMethod = _stateMachineTypeBuilder.DefineMethod("MoveNext", MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual);
                _moveNextMethod.InitLocals = false;

                _moveNextMethodIL = _moveNextMethod.GetILGenerator();

                _methodBuilderField = _stateMachineTypeBuilder.DefineField("_builder", typeof(AsyncTaskMethodBuilder<int>), FieldAttributes.Public);

                _stateField = _stateMachineTypeBuilder.DefineField("_state", _intType, FieldAttributes.Public);
                _stateLocal = _moveNextMethodIL.DeclareLocal(_intType);

                _cancellationTokenField = _stateMachineTypeBuilder.DefineField("_canellationToken", typeof(CancellationToken), FieldAttributes.Public);
                _commandField = _stateMachineTypeBuilder.DefineField("_command", typeof(NpgsqlCommand), FieldAttributes.Public);
                _rootEntityInsertField = _stateMachineTypeBuilder.DefineField("_root" + _rootEntity.EntityName + "Entity", _insertType, FieldAttributes.Public);

                if (shouldLog)
                {
                    _loggerField = _stateMachineTypeBuilder.DefineField("_logger", typeof(Action<CommandType>), FieldAttributes.Public);
                }

                if (isSingleInsert)
                {
                    if (entities.Length == 1)
                    {
                        CreateSingleNoRelationInserter();
                    }
                    else
                    {
                        CreateSingleRelationInserter(entities);
                    }
                }
                else
                {
                    if (entities.Length == 1)
                    {
                        CreateBatchNoRelationInserter();
                    }
                    else
                    {
                        CreateBatchRelationInserter(entities);
                    }
                }

                #region StateMachine

                _stateMachineTypeBuilder.DefineMethodOverride(_moveNextMethod, typeof(IAsyncStateMachine).GetMethod("MoveNext"));

                var setStateMachineMethod = _stateMachineTypeBuilder.DefineMethod("SetStateMachine", MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, null, new[] { typeof(IAsyncStateMachine) });
                setStateMachineMethod.InitLocals = false;

                var setStateMachineMethodIL = setStateMachineMethod.GetILGenerator();

                setStateMachineMethodIL.Emit(OpCodes.Ldarg_0);
                setStateMachineMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
                setStateMachineMethodIL.Emit(OpCodes.Ldarg_1);
                setStateMachineMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetStateMachine"));
                setStateMachineMethodIL.Emit(OpCodes.Ret);

                _stateMachineTypeBuilder.DefineMethodOverride(setStateMachineMethod, typeof(IAsyncStateMachine).GetMethod("SetStateMachine"));

                var parameters = new Type[shouldLog ? 4 : 3];

                parameters[0] = typeof(NpgsqlCommand);
                parameters[1] = _insertType;

                if (shouldLog)
                {
                    parameters[2] = _loggerField.FieldType;
                    parameters[3] = _cancellationTokenField.FieldType;
                }
                else
                {
                    parameters[2] = _cancellationTokenField.FieldType;
                }

                var materializeMethod = _inserterTypeBuilder.DefineMethod("InsertAsync", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, typeof(Task<int>), parameters);
                materializeMethod.InitLocals = false;
                materializeMethod.SetCustomAttribute(new CustomAttributeBuilder(typeof(AsyncStateMachineAttribute).GetConstructor(new[] { typeof(Type) }), new[] { _stateMachineTypeBuilder }));

                var materializeMethodIL = materializeMethod.GetILGenerator();

                var stateMachineLocal = materializeMethodIL.DeclareLocal(_stateMachineTypeBuilder);

                // Create and execute the StateMachine
                materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                materializeMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static));
                materializeMethodIL.Emit(OpCodes.Stfld, _methodBuilderField);
                materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                materializeMethodIL.Emit(OpCodes.Ldarg_0);
                materializeMethodIL.Emit(OpCodes.Stfld, _commandField);
                materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                materializeMethodIL.Emit(OpCodes.Ldarg_1);
                materializeMethodIL.Emit(OpCodes.Stfld, _rootEntityInsertField);

                if (shouldLog)
                {
                    materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                    materializeMethodIL.Emit(OpCodes.Ldarg_2);
                    materializeMethodIL.Emit(OpCodes.Stfld, _loggerField);
                    materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                    materializeMethodIL.Emit(OpCodes.Ldarg_3);
                    materializeMethodIL.Emit(OpCodes.Stfld, _cancellationTokenField);
                }
                else
                {
                    materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                    materializeMethodIL.Emit(OpCodes.Ldarg_2);
                    materializeMethodIL.Emit(OpCodes.Stfld, _cancellationTokenField);
                }

                materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                materializeMethodIL.Emit(OpCodes.Ldc_I4_M1);
                materializeMethodIL.Emit(OpCodes.Stfld, _stateField);
                materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                materializeMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
                materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                materializeMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(_stateMachineTypeBuilder));
                materializeMethodIL.Emit(OpCodes.Ldloca_S, stateMachineLocal);
                materializeMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
                materializeMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetProperty("Task").GetGetMethod());

                materializeMethodIL.Emit(OpCodes.Ret);

                #endregion

                _stateMachineTypeBuilder.CreateType();
                var inserterType = _inserterTypeBuilder.CreateType();

                return inserterType.GetMethod("InsertAsync").CreateDelegate(shouldLog ? typeof(Func<NpgsqlCommand, TInsert, Action<CommandType>, CancellationToken, Task<int>>) : typeof(Func<NpgsqlCommand, TInsert, CancellationToken, Task<int>>));
            }
            else
            {
                var insertMethod = TypeFactory.GetDynamicMethod("InsertAsync", typeof(Task<int>), new[] { typeof(NpgsqlCommand), typeof(TInsert), typeof(CancellationToken) });

                CreateSingleNoRelationNoDbKeysInserter(insertMethod.GetILGenerator());

                return insertMethod.CreateDelegate(typeof(Func<NpgsqlCommand, TInsert, CancellationToken, Task<int>>));
            }
        }

        private void CreateBatchNoRelationInserter()
        {
            var insertedCountLocal = _moveNextMethodIL.DeclareLocal(_intType);

            var retOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();

            // Assign the local state from the local field => state = _state;
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc, _stateLocal);

            // Start try block
            _moveNextMethodIL.BeginExceptionBlock();

            var skipPrimaryKey = _rootEntity.HasDbGeneratedPrimaryKey;

            _moveNextMethodIL.Emit(OpCodes.Ldloc, _stateLocal);
            var switchBuilder = _moveNextMethodIL.EmitSwitch(skipPrimaryKey ? 4 : 1);

            var asyncGenerator = new ILAsyncGenerator(_moveNextMethodIL, switchBuilder, _methodBuilderField, _stateField, _stateLocal, retOfMethodLabel, _stateMachineTypeBuilder);

            // Check if insert is null
            var beforeInvalidRootReturnLabel = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Brfalse, beforeInvalidRootReturnLabel);

            // Check if insert is empty
            var afterInvalidRootReturnLabel = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntityInsertField.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterInvalidRootReturnLabel);

            _moveNextMethodIL.MarkLabel(beforeInvalidRootReturnLabel);

            // Return from method and assign -1 to the insert count
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
            _moveNextMethodIL.Emit(OpCodes.Stloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            _moveNextMethodIL.MarkLabel(afterInvalidRootReturnLabel);

            var commandBuilderLocal = _moveNextMethodIL.DeclareLocal(typeof(StringBuilder));
            var npgsqlCommandLocal = _moveNextMethodIL.DeclareLocal(_commandField.FieldType);

            // Instantiate CommandBuilder
            _moveNextMethodIL.Emit(OpCodes.Newobj, commandBuilderLocal.LocalType.GetConstructor(Type.EmptyTypes));
            _moveNextMethodIL.Emit(OpCodes.Stloc, commandBuilderLocal);

            // Instantiate Command
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
            _moveNextMethodIL.Emit(OpCodes.Stloc, npgsqlCommandLocal);

            var stringBuilder = new StringBuilder();

            var totalColumnCount = _rootEntity.GetColumnCount();
            var columnCount = totalColumnCount - _rootEntity.GetReadOnlyCount();
            var columnOffset = skipPrimaryKey ? _rootEntity.GetRegularColumnOffset() : 0;
            var lastNonReadOnlyIndex = _rootEntity.GetLastRegularColumnsIndex();

            stringBuilder.Append("INSERT INTO ")
                         .Append(_rootEntity.TableName)
                         .Append(" (")
                         .Append(skipPrimaryKey ? _rootEntity.NonPrimaryColumnListString : _rootEntity.ColumnListString)
                         .Append(") VALUES ");

            // Outer loop to keep one single command under ushort.MaxValue parameters

            var totalLocal = _moveNextMethodIL.DeclareLocal(_intType);
            var currentLocal = _moveNextMethodIL.DeclareLocal(_intType);

            // Assign the total amount of parameters to the total local
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntityInsertField.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Stloc, totalLocal);

            // Assign 0 to the current local
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
            _moveNextMethodIL.Emit(OpCodes.Stloc, currentLocal);

            var outerIteratorLocal = _moveNextMethodIL.DeclareLocal(_intType);

            var outerLoopConditionLabel = _moveNextMethodIL.DefineLabel();
            var outerStartLoopBodyLabel = _moveNextMethodIL.DefineLabel();

            // Assign 0 to the iterator
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
            _moveNextMethodIL.Emit(OpCodes.Stloc, outerIteratorLocal);
            _moveNextMethodIL.Emit(OpCodes.Br, outerLoopConditionLabel);

            // loop body
            _moveNextMethodIL.MarkLabel(outerStartLoopBodyLabel);

            // Append base Insert Command to command builder
            _moveNextMethodIL.Emit(OpCodes.Ldloc, commandBuilderLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldstr, stringBuilder.ToString());
            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetMethod("Append", new[] { typeof(string) }));
            _moveNextMethodIL.Emit(OpCodes.Pop);

            var leftLocal = _moveNextMethodIL.DeclareLocal(_intType);

            var totalColumns = columnCount - columnOffset;

            // Assign the amount of left items to the left local
            _moveNextMethodIL.Emit(OpCodes.Ldloc, totalLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
            _moveNextMethodIL.Emit(OpCodes.Mul);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
            _moveNextMethodIL.Emit(OpCodes.Mul);
            _moveNextMethodIL.Emit(OpCodes.Sub);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4, ushort.MaxValue);
            _moveNextMethodIL.Emit(OpCodes.Call, typeof(Math).GetMethod("Min", new[] { _intType, _intType }));
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
            _moveNextMethodIL.Emit(OpCodes.Div);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
            _moveNextMethodIL.Emit(OpCodes.Add);
            _moveNextMethodIL.Emit(OpCodes.Stloc, leftLocal);

            var iteratorElementLocal = _moveNextMethodIL.DeclareLocal(_rootEntity.EntityType);

            var loopConditionLabel = _moveNextMethodIL.DefineLabel();
            var startLoopBodyLabel = _moveNextMethodIL.DefineLabel();

            // loop body
            _moveNextMethodIL.MarkLabel(startLoopBodyLabel);

            // get element at iterator from list
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntityInsertField.FieldType.GetMethod("get_Item"));
            _moveNextMethodIL.Emit(OpCodes.Stloc, iteratorElementLocal);

            // append placeholders to command builder
            _moveNextMethodIL.Emit(OpCodes.Ldloc, commandBuilderLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)'(');
            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetMethod("Append", new[] { typeof(char) }));
            _moveNextMethodIL.Emit(OpCodes.Pop);

            for (var k = columnOffset; k <= lastNonReadOnlyIndex; k++)
            {
                var column = _rootEntity.GetColumn(k);

                if (column.Options.HasFlag(ColumnOptions.ReadOnly))
                    continue;

                // Write placeholder to the command builder => (@Name(n)),
                _moveNextMethodIL.Emit(OpCodes.Ldloc, commandBuilderLocal);

                if (column.Options.HasFlag(ColumnOptions.DefaultValue))
                {
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, lastNonReadOnlyIndex == k ? "DEFAULT), " : "DEFAULT, ");
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Pop);
                }
                else
                {
                    // Create new parameter with placeholder and add it to the parameter list
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, npgsqlCommandLocal);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, npgsqlCommandLocal.LocalType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());

                    WriteNpgsqlParameterFromColumn(_moveNextMethodIL, iteratorElementLocal, column, currentLocal);

                    _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameterCollection).GetMethod("Add", new[] { typeof(NpgsqlParameter) }));

                    // Write placeholder to the command builder => (@Name(n)),
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameter).GetProperty("ParameterName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, lastNonReadOnlyIndex == k ? "), " : ", ");
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Pop);
                }
            }

            // loop iterator increment
            _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
            _moveNextMethodIL.Emit(OpCodes.Add);
            _moveNextMethodIL.Emit(OpCodes.Stloc, currentLocal);

            // loop condition
            _moveNextMethodIL.MarkLabel(loopConditionLabel);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, leftLocal);
            _moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

            // Remove the last the values form the command string e.g. ", "
            _moveNextMethodIL.Emit(OpCodes.Ldloc, commandBuilderLocal);
            _moveNextMethodIL.Emit(OpCodes.Dup);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetProperty("Length").GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_2);
            _moveNextMethodIL.Emit(OpCodes.Sub);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetProperty("Length").GetSetMethod());

            if (skipPrimaryKey)
            {
                // Append " RETURNING \"PrimaryKey\""
                _moveNextMethodIL.Emit(OpCodes.Ldloc, commandBuilderLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldstr, " RETURNING " + _rootEntity.GetPrimaryColumn().NormalizedColumnName + ";");
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetMethod("Append", new[] { typeof(string) }));
                _moveNextMethodIL.Emit(OpCodes.Pop);
            }
            else
            {
                // Append ";"
                _moveNextMethodIL.Emit(OpCodes.Ldloc, commandBuilderLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)';');
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetMethod("Append", new[] { typeof(char) }));
                _moveNextMethodIL.Emit(OpCodes.Pop);
            }


            // outer loop iterator increment
            _moveNextMethodIL.Emit(OpCodes.Ldloc, outerIteratorLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
            _moveNextMethodIL.Emit(OpCodes.Add);
            _moveNextMethodIL.Emit(OpCodes.Stloc, outerIteratorLocal);

            // outer loop condition
            _moveNextMethodIL.MarkLabel(outerLoopConditionLabel);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, totalLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
            _moveNextMethodIL.Emit(OpCodes.Bne_Un, outerStartLoopBodyLabel);

            // Assign the commandBuilder text to the command
            _moveNextMethodIL.Emit(OpCodes.Ldloc, npgsqlCommandLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, commandBuilderLocal);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderLocal.LocalType.GetMethod("ToString", Type.EmptyTypes));
            _moveNextMethodIL.Emit(OpCodes.Callvirt, npgsqlCommandLocal.LocalType.GetProperty("CommandText").GetSetMethod());

            FieldBuilder? dataReaderField = default;

            if (skipPrimaryKey)
            {
                var dataReaderTaskAwaiterField = _stateMachineTypeBuilder.DefineField("_objectTaskAwaiter", typeof(TaskAwaiter<NpgsqlDataReader>), FieldAttributes.Private);
                var dataReaderTaskAwaiterLocal = _moveNextMethodIL.DeclareLocal(dataReaderTaskAwaiterField.FieldType);

                var boolTaskAwaiterField = _stateMachineTypeBuilder.DefineField("_boolTaskAwaiter", typeof(TaskAwaiter<bool>), FieldAttributes.Private);
                var boolTaskAwaiterLocal = _moveNextMethodIL.DeclareLocal(boolTaskAwaiterField.FieldType);

                dataReaderField = _stateMachineTypeBuilder.DefineField("_dataReader", typeof(NpgsqlDataReader), FieldAttributes.Private);

                // Get the result of the command
                _moveNextMethodIL.Emit(OpCodes.Ldloc, npgsqlCommandLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteReaderAsync", new[] { _cancellationTokenField.FieldType }));

                asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<NpgsqlDataReader>), dataReaderTaskAwaiterLocal, dataReaderTaskAwaiterField);

                var dataReaderLocal = _moveNextMethodIL.DeclareLocal(dataReaderField.FieldType);

                _moveNextMethodIL.Emit(OpCodes.Stloc, dataReaderLocal);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, dataReaderLocal);
                _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);

                var iteratorField = _stateMachineTypeBuilder.DefineField("_iterator", _intType, FieldAttributes.Private);
                var counterField = _stateMachineTypeBuilder.DefineField("_counter", typeof(ushort), FieldAttributes.Private);

                loopConditionLabel = _moveNextMethodIL.DefineLabel();
                startLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                // Assign 0 to the counter
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                // Assign 0 to the iterator
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Stfld, iteratorField);
                _moveNextMethodIL.Emit(OpCodes.Br, loopConditionLabel);

                // loop body
                _moveNextMethodIL.MarkLabel(startLoopBodyLabel);

                // check if counter is equal to ushort.MaxValue

                var afterIfBody = _moveNextMethodIL.DefineLabel();

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, counterField);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, ushort.MaxValue / totalColumns);
                _moveNextMethodIL.Emit(OpCodes.Bne_Un, afterIfBody);

                // Assign 0 to the counter
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                // Call the next result
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("NextResultAsync", new[] { _cancellationTokenField.FieldType }));

                asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), boolTaskAwaiterLocal, boolTaskAwaiterField);

                _moveNextMethodIL.Emit(OpCodes.Pop);

                _moveNextMethodIL.MarkLabel(afterIfBody);

                // read data reader

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }));

                asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), boolTaskAwaiterLocal, boolTaskAwaiterField);

                _moveNextMethodIL.Emit(OpCodes.Pop);

                // assign the returned id to the current element
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntityInsertField.FieldType.GetMethod("get_Item"));
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("GetFieldValue").MakeGenericMethod(_rootEntity.GetPrimaryColumn().PropertyInfo.PropertyType));

                WritePropertyAssigner(_moveNextMethodIL, _rootEntity.GetPrimaryColumn());

                // loop iterator increment
                var tempIteratorLocal = _moveNextMethodIL.DeclareLocal(_intType);
                var tempCounterLocal = _moveNextMethodIL.DeclareLocal(_intType);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                _moveNextMethodIL.Emit(OpCodes.Stloc, tempIteratorLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, tempIteratorLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                _moveNextMethodIL.Emit(OpCodes.Add);
                _moveNextMethodIL.Emit(OpCodes.Stfld, iteratorField);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, counterField);
                _moveNextMethodIL.Emit(OpCodes.Stloc, tempCounterLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, tempCounterLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                _moveNextMethodIL.Emit(OpCodes.Add);
                _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                // loop condition
                _moveNextMethodIL.MarkLabel(loopConditionLabel);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntityInsertField.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                _moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

                // dispose data reader
                var valueTaskAwaiterField = _stateMachineTypeBuilder.DefineField("_valueTaskAwaiter", typeof(ValueTaskAwaiter), FieldAttributes.Private);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("DisposeAsync"));

                asyncGenerator.WriteAsyncValueTaskMethodAwaiter(_moveNextMethodIL.DeclareLocal(typeof(ValueTask)), _moveNextMethodIL.DeclareLocal(valueTaskAwaiterField.FieldType), valueTaskAwaiterField);

                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);
            }
            else
            {
                // Get the result of the command
                _moveNextMethodIL.Emit(OpCodes.Ldloc, npgsqlCommandLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteNonQueryAsync", new[] { _cancellationTokenField.FieldType }));

                asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<int>), _moveNextMethodIL.DeclareLocal(typeof(TaskAwaiter<int>)), _stateMachineTypeBuilder.DefineField("_intTaskAwaiter", typeof(TaskAwaiter<int>), FieldAttributes.Private));

                _moveNextMethodIL.Emit(OpCodes.Pop);
            }

            if (_loggerField is not null)
            {
                // log success of insert
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _loggerField);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)CommandType.InsertBatch);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, _loggerField.FieldType.GetMethod("Invoke"));
            }

            // return the amount of inserted rows
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntityInsertField.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Stloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            var exceptionLocal = _moveNextMethodIL.DeclareLocal(typeof(Exception));

            _moveNextMethodIL.BeginCatchBlock(exceptionLocal.LocalType);

            // Set state and return exception
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);

            if (dataReaderField is not null)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);
            }

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

            // End of catch block
            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfMethodLabel);

            // Set state and return result
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);

            if (dataReaderField is not null)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);
            }

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetResult"));

            // End of method
            _moveNextMethodIL.MarkLabel(retOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);
        }

        private void CreateBatchRelationInserter(EntityRelationHolder[] entities)
        {
            FieldBuilder? dataReaderTaskAwaiterField = default;
            FieldBuilder? boolTaskAwaiterField = default;
            FieldBuilder? intTaskAwaiterField = default;
            FieldBuilder? valueTaskAwaiterField = default;
            FieldBuilder? dataReaderField = default;
            FieldBuilder? iteratorField = default;
            FieldBuilder? counterField = default;

            LocalBuilder? dataReaderTaskAwaiterLocal = default;
            LocalBuilder? boolTaskAwaiterLocal = default;
            LocalBuilder? intTaskAwaiterLocal = default;
            LocalBuilder? valueTaskAwaiterLocal = default;

            var insertedCountLocal = _moveNextMethodIL.DeclareLocal(_intType);

            var retOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();

            // Assign the local state from the local field => state = _state;
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc, _stateLocal);

            // Start try block
            _moveNextMethodIL.BeginExceptionBlock();

            _moveNextMethodIL.Emit(OpCodes.Ldloc, _stateLocal);

            var awaiterCount = 0;

            for (var entityIndex = entities.Length - 1; entityIndex >= 0; entityIndex--)
            {
                awaiterCount += entities[entityIndex].Entity.HasDbGeneratedPrimaryKey ? 4 : 1;
            }

            var switchBuilder = _moveNextMethodIL.EmitSwitch(awaiterCount);

            var asyncGenerator = new ILAsyncGenerator(_moveNextMethodIL, switchBuilder, _methodBuilderField, _stateField, _stateLocal, retOfMethodLabel, _stateMachineTypeBuilder);

            // Check if insert is null
            var beforeInvalidRootReturnLabel = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Brfalse, beforeInvalidRootReturnLabel);

            // Check if insert is empty
            var afterInvalidRootReturnLabel = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);

            _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntityInsertField.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterInvalidRootReturnLabel);

            _moveNextMethodIL.MarkLabel(beforeInvalidRootReturnLabel);

            // Return from method and assign -1 to the insert count
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
            _moveNextMethodIL.Emit(OpCodes.Stloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            _moveNextMethodIL.MarkLabel(afterInvalidRootReturnLabel);

            var entityCollections = new EntitySeprator(_moveNextMethodIL, _stateMachineTypeBuilder, _rootEntity, entities, _reachableEntities, _reachableRelations).WriteEntitySeperater(_rootEntityInsertField);

            var commandBuilderField = _stateMachineTypeBuilder.DefineField("commandBuilder", typeof(StringBuilder), FieldAttributes.Private);

            // Instantiate CommandBuilder
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Newobj, commandBuilderField.FieldType.GetConstructor(Type.EmptyTypes));
            _moveNextMethodIL.Emit(OpCodes.Stfld, commandBuilderField);

            for (var entityIndex = 0; entityIndex < entities.Length; entityIndex++)
            {
                var entityHolder = entities[entityIndex];
                var entity = entityHolder.Entity;

                FieldBuilder entityCollection;
                Label? endOfEntityInsertLabel;

                if (entity == _rootEntity)
                {
                    entityCollection = _rootEntityInsertField;
                    endOfEntityInsertLabel = default;
                }
                else
                {
                    var entityId = _reachableEntities.HasId(entity, out _);

                    entityCollection = entityCollections[entityId];

                    // Check if entityCollection is larger than 0
                    endOfEntityInsertLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Brfalse, endOfEntityInsertLabel.Value);
                }

                // Clear commandBuilder and command parameters
                if (entityIndex > 0)
                {
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Clear"));
                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).PropertyType.GetMethod("Clear"));
                }

                var stringBuilder = new StringBuilder();

                var skipPrimaryKey = entity.HasDbGeneratedPrimaryKey;

                var totalColumnCount = entity.GetColumnCount();
                var columnCount = totalColumnCount - entity.GetReadOnlyCount();
                var columnOffset = skipPrimaryKey ? entity.GetRegularColumnOffset() : 0;
                var lastNonReadOnlyIndex = entity.GetLastRegularColumnsIndex();

                stringBuilder.Append("INSERT INTO ")
                             .Append(entity.TableName)
                             .Append(" (")
                             .Append(skipPrimaryKey ? entity.NonPrimaryColumnListString : entity.ColumnListString)
                             .Append(") VALUES ");

                // Outer loop to keep one single command under ushort.MaxValue parameters

                var totalLocal = _moveNextMethodIL.DeclareLocal(_intType);
                var currentLocal = _moveNextMethodIL.DeclareLocal(_intType);

                // Assign the total amount of parameters to the total local
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                _moveNextMethodIL.Emit(OpCodes.Stloc, totalLocal);

                // Assign 0 to the current local
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Stloc, currentLocal);

                var outerIteratorLocal = _moveNextMethodIL.DeclareLocal(_intType);

                var outerLoopConditionLabel = _moveNextMethodIL.DefineLabel();
                var outerStartLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                // Assign 0 to the iterator
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                _moveNextMethodIL.Emit(OpCodes.Stloc, outerIteratorLocal);
                _moveNextMethodIL.Emit(OpCodes.Br, outerLoopConditionLabel);

                // loop body
                _moveNextMethodIL.MarkLabel(outerStartLoopBodyLabel);

                // Append base Insert Command to command builder
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                _moveNextMethodIL.Emit(OpCodes.Ldstr, stringBuilder.ToString());
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                _moveNextMethodIL.Emit(OpCodes.Pop);

                var leftLocal = _moveNextMethodIL.DeclareLocal(_intType);

                var totalColumns = columnCount - columnOffset;

                // Assign the amount of left items to the left local
                _moveNextMethodIL.Emit(OpCodes.Ldloc, totalLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
                _moveNextMethodIL.Emit(OpCodes.Mul);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
                _moveNextMethodIL.Emit(OpCodes.Mul);
                _moveNextMethodIL.Emit(OpCodes.Sub);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, ushort.MaxValue);
                _moveNextMethodIL.Emit(OpCodes.Call, typeof(Math).GetMethod("Min", new[] { _intType, _intType }));
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
                _moveNextMethodIL.Emit(OpCodes.Div);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                _moveNextMethodIL.Emit(OpCodes.Add);
                _moveNextMethodIL.Emit(OpCodes.Stloc, leftLocal);

                var iteratorElementLocal = _moveNextMethodIL.DeclareLocal(entity.EntityType);

                var loopConditionLabel = _moveNextMethodIL.DefineLabel();
                var startLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                // loop body
                _moveNextMethodIL.MarkLabel(startLoopBodyLabel);

                // get element at iterator from list
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.GetMethod("get_Item"));
                _moveNextMethodIL.Emit(OpCodes.Stloc, iteratorElementLocal);

                // assign foreign key to itself from navigation property primary key
                for (var relationIndex = entityHolder.SelfAssignedRelations.Count - 1; relationIndex >= 0; relationIndex--)
                {
                    var relation = entityHolder.SelfAssignedRelations[relationIndex];

                    // Check if navigation property is null

                    var afterBodyLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Brfalse, afterBodyLabel);

                    _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                    WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);

                    _moveNextMethodIL.MarkLabel(afterBodyLabel);
                }

                // append placeholders to command builder
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)'(');
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(char) }));
                _moveNextMethodIL.Emit(OpCodes.Pop);

                for (var k = columnOffset; k <= lastNonReadOnlyIndex; k++)
                {
                    var column = entity.GetColumn(k);

                    if (column.Options.HasFlag(ColumnOptions.ReadOnly))
                        continue;

                    // Write placeholder to the command builder => (@Name(n)),
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);

                    if (column.Options.HasFlag(ColumnOptions.DefaultValue))
                    {
                        _moveNextMethodIL.Emit(OpCodes.Ldstr, lastNonReadOnlyIndex == k ? "DEFAULT), " : "DEFAULT, ");
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                        _moveNextMethodIL.Emit(OpCodes.Pop);
                    }
                    else
                    {
                        // Create new parameter with placeholder and add it to the parameter list
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());

                        WriteNpgsqlParameterFromColumn(_moveNextMethodIL, iteratorElementLocal, column, currentLocal);

                        _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameterCollection).GetMethod("Add", new[] { typeof(NpgsqlParameter) }));

                        // Write placeholder to the command builder => (@Name(n)),
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameter).GetProperty("ParameterName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                        _moveNextMethodIL.Emit(OpCodes.Ldstr, lastNonReadOnlyIndex == k ? "), " : ", ");
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                        _moveNextMethodIL.Emit(OpCodes.Pop);
                    }
                }

                // loop iterator increment
                _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                _moveNextMethodIL.Emit(OpCodes.Add);
                _moveNextMethodIL.Emit(OpCodes.Stloc, currentLocal);

                // loop condition
                _moveNextMethodIL.MarkLabel(loopConditionLabel);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, leftLocal);
                _moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

                // Remove the last the values form the command string e.g. ", "
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                _moveNextMethodIL.Emit(OpCodes.Dup);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetProperty("Length").GetGetMethod());
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_2);
                _moveNextMethodIL.Emit(OpCodes.Sub);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetProperty("Length").GetSetMethod());

                if (skipPrimaryKey)
                {
                    // Append " RETURNING \"PrimaryKey\""
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, " RETURNING " + entity.GetPrimaryColumn().NormalizedColumnName + ";");
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Pop);
                }

                // outer loop iterator increment
                _moveNextMethodIL.Emit(OpCodes.Ldloc, outerIteratorLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                _moveNextMethodIL.Emit(OpCodes.Add);
                _moveNextMethodIL.Emit(OpCodes.Stloc, outerIteratorLocal);

                // outer loop condition
                _moveNextMethodIL.MarkLabel(outerLoopConditionLabel);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, totalLocal);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                _moveNextMethodIL.Emit(OpCodes.Bne_Un, outerStartLoopBodyLabel);

                // Assign the commandBuilder text to the command
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("ToString", Type.EmptyTypes));
                _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("CommandText").GetSetMethod());

                if (skipPrimaryKey)
                {
                    dataReaderField ??= _stateMachineTypeBuilder.DefineField("_dataReader", typeof(NpgsqlDataReader), FieldAttributes.Private);

                    // Get the result of the command
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteReaderAsync", new[] { _cancellationTokenField.FieldType }));

                    dataReaderTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_dataReaderTaskAwaiter", typeof(TaskAwaiter<NpgsqlDataReader>), FieldAttributes.Private);
                    dataReaderTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(dataReaderTaskAwaiterField.FieldType);

                    asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<NpgsqlDataReader>), dataReaderTaskAwaiterLocal, dataReaderTaskAwaiterField);

                    var dataReaderLocal = _moveNextMethodIL.DeclareLocal(dataReaderField.FieldType);

                    _moveNextMethodIL.Emit(OpCodes.Stloc, dataReaderLocal);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, dataReaderLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);

                    iteratorField ??= _stateMachineTypeBuilder.DefineField("_iterator", _intType, FieldAttributes.Private);
                    counterField ??= _stateMachineTypeBuilder.DefineField("_counter", typeof(ushort), FieldAttributes.Private);

                    loopConditionLabel = _moveNextMethodIL.DefineLabel();
                    startLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                    // Assign 0 to the counter
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                    // Assign 0 to the iterator
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, iteratorField);
                    _moveNextMethodIL.Emit(OpCodes.Br, loopConditionLabel);

                    // loop body
                    _moveNextMethodIL.MarkLabel(startLoopBodyLabel);

                    // check if counter is equal to ushort.MaxValue

                    var afterIfBody = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, counterField);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, ushort.MaxValue / totalColumns);
                    _moveNextMethodIL.Emit(OpCodes.Bne_Un, afterIfBody);

                    // Assign 0 to the counter
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                    // Call the next result
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("NextResultAsync", new[] { _cancellationTokenField.FieldType }));

                    boolTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_boolTaskAwaiter", typeof(TaskAwaiter<bool>), FieldAttributes.Private);
                    boolTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(boolTaskAwaiterField.FieldType);

                    asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), boolTaskAwaiterLocal, boolTaskAwaiterField);

                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    _moveNextMethodIL.MarkLabel(afterIfBody);

                    // read data reader

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }));

                    asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), boolTaskAwaiterLocal, boolTaskAwaiterField);

                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    // assign foreign key to itself from navigation property primary key
                    if (entityHolder.ForeignAssignedRelations.Count > 0)
                    {
                        var primaryKeyLocal = _moveNextMethodIL.DeclareLocal(entity.GetPrimaryColumn().PropertyInfo.PropertyType);

                        // assign the returned id to the local
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("GetFieldValue").MakeGenericMethod(entity.GetPrimaryColumn().PropertyInfo.PropertyType));
                        _moveNextMethodIL.Emit(OpCodes.Stloc, primaryKeyLocal);

                        var entityLocal = _moveNextMethodIL.DeclareLocal(entity.EntityType);

                        // assign the current entity to the local
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.GetMethod("get_Item"));
                        _moveNextMethodIL.Emit(OpCodes.Stloc, entityLocal);

                        // assign the returned id to the current entity
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                        WritePropertyAssigner(_moveNextMethodIL, entity.GetPrimaryColumn());

                        LocalBuilder? innerIteratorLocal = default;

                        for (var relationIndex = entityHolder.ForeignAssignedRelations.Count - 1; relationIndex >= 0; relationIndex--)
                        {
                            var relation = entityHolder.ForeignAssignedRelations[relationIndex];

                            var afterOuterNullCheckBodyLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterOuterNullCheckBodyLabel);

                            if (relation.RelationType == RelationType.OneToMany)
                            {
                                innerIteratorLocal ??= _moveNextMethodIL.DeclareLocal(_intType);

                                var innerLoopConditionLabel = _moveNextMethodIL.DefineLabel();
                                var innertStartLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                                var foreignEntityLocal = _moveNextMethodIL.DeclareLocal(relation.RightEntity.EntityType);

                                // Assign 0 to the iterator
                                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                                _moveNextMethodIL.Emit(OpCodes.Stloc, innerIteratorLocal);
                                _moveNextMethodIL.Emit(OpCodes.Br, innerLoopConditionLabel);

                                // loop body
                                _moveNextMethodIL.MarkLabel(innertStartLoopBodyLabel);

                                // assign iterator element to foreignEntityLocal
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetMethod("get_Item"));
                                _moveNextMethodIL.Emit(OpCodes.Stloc, foreignEntityLocal);

                                // check if element is not null
                                var afterNullCheckBodyLabel = _moveNextMethodIL.DefineLabel();

                                _moveNextMethodIL.Emit(OpCodes.Ldloc, foreignEntityLocal);
                                _moveNextMethodIL.Emit(OpCodes.Brfalse, afterNullCheckBodyLabel);

                                // assign entity primary key to foreign key on navigation entity
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, foreignEntityLocal);
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                                WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);

                                _moveNextMethodIL.MarkLabel(afterNullCheckBodyLabel);

                                // loop iterator increment
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                                _moveNextMethodIL.Emit(OpCodes.Add);
                                _moveNextMethodIL.Emit(OpCodes.Stloc, innerIteratorLocal);

                                // loop condition
                                _moveNextMethodIL.MarkLabel(innerLoopConditionLabel);
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Blt, innertStartLoopBodyLabel);

                            }
                            else
                            {
                                // assign entity primary key to foreign key on navigation entity
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                                WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);
                            }

                            _moveNextMethodIL.MarkLabel(afterOuterNullCheckBodyLabel);
                        }
                    }
                    else
                    {
                        // assign the returned id to the current element
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.GetMethod("get_Item"));
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("GetFieldValue").MakeGenericMethod(entity.GetPrimaryColumn().PropertyInfo.PropertyType));
                        WritePropertyAssigner(_moveNextMethodIL, entity.GetPrimaryColumn());
                    }

                    // loop iterator increment
                    var tempIteratorLocal = _moveNextMethodIL.DeclareLocal(_intType);
                    var tempCounterLocal = _moveNextMethodIL.DeclareLocal(_intType);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, tempIteratorLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, tempIteratorLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    _moveNextMethodIL.Emit(OpCodes.Add);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, iteratorField);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, counterField);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, tempCounterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, tempCounterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    _moveNextMethodIL.Emit(OpCodes.Add);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                    // loop condition
                    _moveNextMethodIL.MarkLabel(loopConditionLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

                    // dispose data reader
                    valueTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_valueTaskAwaiter", typeof(ValueTaskAwaiter), FieldAttributes.Private);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("DisposeAsync"));

                    asyncGenerator.WriteAsyncValueTaskMethodAwaiter(_moveNextMethodIL.DeclareLocal(typeof(ValueTask)), valueTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(valueTaskAwaiterField.FieldType), valueTaskAwaiterField);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldnull);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);
                }
                else
                {
                    // Get the result of the command
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteNonQueryAsync", new[] { _cancellationTokenField.FieldType }));

                    intTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_intTaskAwaiter", typeof(TaskAwaiter<int>), FieldAttributes.Private);
                    intTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(intTaskAwaiterField.FieldType);

                    asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<int>), intTaskAwaiterLocal, intTaskAwaiterField);

                    _moveNextMethodIL.Emit(OpCodes.Pop);
                }

                if (_loggerField is not null)
                {
                    // log success of insert
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _loggerField);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)CommandType.InsertBatch);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _loggerField.FieldType.GetMethod("Invoke"));
                }

                if (endOfEntityInsertLabel.HasValue)
                {
                    _moveNextMethodIL.MarkLabel(endOfEntityInsertLabel.Value);
                }
            }

            // return the amount of inserted rows
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntityInsertField.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());

            foreach (var entityCollection in entityCollections.Values)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                _moveNextMethodIL.Emit(OpCodes.Add);
            }

            _moveNextMethodIL.Emit(OpCodes.Stloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            var exceptionLocal = _moveNextMethodIL.DeclareLocal(typeof(Exception));

            _moveNextMethodIL.BeginCatchBlock(exceptionLocal.LocalType);

            // Set state and return exception
            _moveNextMethodIL.Emit(OpCodes.Stloc, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);

            if (dataReaderField is not null)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);
            }

            foreach (var entityCollection in entityCollections.Values)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, entityCollection);
            }

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

            // End of catch block
            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfMethodLabel);

            // Set state and return result
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);

            if (dataReaderField is not null)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);
            }

            foreach (var entityCollection in entityCollections.Values)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldnull);
                _moveNextMethodIL.Emit(OpCodes.Stfld, entityCollection);
            }

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetResult"));

            // End of method
            _moveNextMethodIL.MarkLabel(retOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);
        }

        private void CreateSingleNoRelationInserter()
        {
            var objectTaskAwaiterField = _stateMachineTypeBuilder.DefineField("_objectTaskAwaiter", typeof(TaskAwaiter<object>), FieldAttributes.Private);

            var objectTaskAwaiterLocal = _moveNextMethodIL.DeclareLocal(objectTaskAwaiterField.FieldType);
            var insertedCountLocal = _moveNextMethodIL.DeclareLocal(_intType);

            var retOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();

            // Assign the local state from the local field => state = _state;
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc, _stateLocal);

            // Start try block
            _moveNextMethodIL.BeginExceptionBlock();

            // if state zero goto await unsafe

            var switchBuilder = new ILSwitchBuilder(_moveNextMethodIL, 1);

            _moveNextMethodIL.Emit(OpCodes.Ldloc, _stateLocal);
            _moveNextMethodIL.Emit(OpCodes.Brfalse, switchBuilder.GetLabels()[0]);

            // Check if insert is null
            var afterRootInsertNullCheck = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterRootInsertNullCheck);

            // Return from method and assign -1 to the insert count
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
            _moveNextMethodIL.Emit(OpCodes.Stloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            _moveNextMethodIL.MarkLabel(afterRootInsertNullCheck);

            // Create command and assign sql command and connection
            var sqlBuilder = new StringBuilder();

            var skipPrimaryKey = _rootEntity.HasDbGeneratedPrimaryKey;

            sqlBuilder.Append("INSERT INTO ")
                      .Append(_rootEntity.TableName)
                      .Append(" (")
                      .Append(skipPrimaryKey ? _rootEntity.NonPrimaryColumnListString : _rootEntity.ColumnListString)
                      .Append(") VALUES (");

            var columnOffset = skipPrimaryKey ? _rootEntity.GetRegularColumnOffset() : 0;
            var lastNonReadOnlyIndex = _rootEntity.GetLastRegularColumnsIndex();

            for (var columnIndex = columnOffset; columnIndex <= lastNonReadOnlyIndex; columnIndex++)
            {
                var column = _rootEntity.GetColumn(columnIndex);

                if (column.Options.HasFlag(ColumnOptions.ReadOnly))
                    continue;

                if (column.Options.HasFlag(ColumnOptions.DefaultValue))
                {
                    sqlBuilder.Append("DEFAULT, ");
                }
                else
                {
                    sqlBuilder.Append('@')
                              .Append(column.PropertyInfo.Name)
                              .Append(", ");
                }
            }

            sqlBuilder.Length -= 2;

            if (skipPrimaryKey)
            {
                sqlBuilder.Append(") RETURNING ")
                          .Append(_rootEntity.GetPrimaryColumn().NormalizedColumnName)
                          .Append(";");
            }
            else
            {
                sqlBuilder.Append(");");
            }

            // Assign commandText to command
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
            _moveNextMethodIL.Emit(OpCodes.Dup);
            _moveNextMethodIL.Emit(OpCodes.Ldstr, sqlBuilder.ToString());
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("CommandText").GetSetMethod());

            // Assign parameters to command
            for (var columnIndex = columnOffset; columnIndex <= lastNonReadOnlyIndex; columnIndex++)
            {
                var column = _rootEntity.GetColumn(columnIndex);

                if (column.Options.HasFlag(ColumnOptions.ReadOnly) ||
                    column.Options.HasFlag(ColumnOptions.DefaultValue))
                    continue;

                _moveNextMethodIL.Emit(OpCodes.Dup);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                WriteNpgsqlParameterFromColumn(_moveNextMethodIL, _rootEntityInsertField, column);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameterCollection).GetMethod("Add", new[] { typeof(NpgsqlParameter) }));
                _moveNextMethodIL.Emit(OpCodes.Pop);
            }

            // Get the result of the command
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteScalarAsync", new[] { _cancellationTokenField.FieldType }));

            new ILAsyncGenerator(_moveNextMethodIL, switchBuilder, _methodBuilderField, _stateField, _stateLocal, retOfMethodLabel, _stateMachineTypeBuilder).WriteAsyncMethodAwaiter(typeof(Task<object>), objectTaskAwaiterLocal, objectTaskAwaiterField);

            var objectResultLocal = _moveNextMethodIL.DeclareLocal(typeof(object));

            _moveNextMethodIL.Emit(OpCodes.Stloc, objectResultLocal);

            // cast and store primary key
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, objectResultLocal);
            _moveNextMethodIL.Emit(OpCodes.Unbox_Any, _rootEntity.GetPrimaryColumn().PropertyInfo.PropertyType);

            WritePropertyAssigner(_moveNextMethodIL, _rootEntity.GetPrimaryColumn());

            if (_loggerField is not null)
            {
                // log success of insert
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, _loggerField);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)CommandType.InsertSingle);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, _loggerField.FieldType.GetMethod("Invoke"));
            }

            // return 1
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
            _moveNextMethodIL.Emit(OpCodes.Stloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            var exceptionLocal = _moveNextMethodIL.DeclareLocal(typeof(Exception));

            _moveNextMethodIL.BeginCatchBlock(exceptionLocal.LocalType);
            // Set state and return exception
            _moveNextMethodIL.Emit(OpCodes.Stloc_S, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

            // End of catch block
            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfMethodLabel);

            // Set state and return result
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc_S, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetResult"));

            // End of method
            _moveNextMethodIL.MarkLabel(retOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);
        }

        private void CreateSingleRelationInserter(EntityRelationHolder[] entities)
        {
            FieldBuilder? dataReaderTaskAwaiterField = default;
            FieldBuilder? boolTaskAwaiterField = default;
            FieldBuilder? intTaskAwaiterField = default;
            FieldBuilder? valueTaskAwaiterField = default;
            FieldBuilder? dataReaderField = default;
            FieldBuilder? iteratorField = default;
            FieldBuilder? counterField = default;

            LocalBuilder? dataReaderTaskAwaiterLocal = default;
            LocalBuilder? boolTaskAwaiterLocal = default;
            LocalBuilder? intTaskAwaiterLocal = default;
            LocalBuilder? valueTaskAwaiterLocal = default;

            var insertedCountLocal = _moveNextMethodIL.DeclareLocal(_intType);

            var retOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();

            // Assign the local state from the local field => state = _state;
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc, _stateLocal);

            // Start try block
            _moveNextMethodIL.BeginExceptionBlock();

            // if state zero goto await unsafe

            var awaiterCount = 0;

            for (var entityIndex = entities.Length - 1; entityIndex >= 0; entityIndex--)
            {
                var entity = entities[entityIndex].Entity;

                if (entity == _rootEntity)
                {
                    awaiterCount++;

                    continue;
                }

                awaiterCount += entity.HasDbGeneratedPrimaryKey ? 4 : 1;
            }

            _moveNextMethodIL.Emit(OpCodes.Ldloc, _stateLocal);

            var switchBuilder = _moveNextMethodIL.EmitSwitch(awaiterCount);

            var asyncGenerator = new ILAsyncGenerator(_moveNextMethodIL, switchBuilder, _methodBuilderField, _stateField, _stateLocal, retOfMethodLabel, _stateMachineTypeBuilder);

            // Check if insert is null
            var afterRootInsertNullCheck = _moveNextMethodIL.DefineLabel();

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterRootInsertNullCheck);

            // Return from method and assign -1 to the insert count
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
            _moveNextMethodIL.Emit(OpCodes.Stloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            _moveNextMethodIL.MarkLabel(afterRootInsertNullCheck);

            var entityCollections = new EntitySeprator(_moveNextMethodIL, _stateMachineTypeBuilder, _rootEntity, entities, _reachableEntities, _reachableRelations).WriteFlatEntitySeperater(_rootEntityInsertField);

            var commandBuilderField = _stateMachineTypeBuilder.DefineField("commandBuilder", typeof(StringBuilder), FieldAttributes.Private);

            // Instantiate CommandBuilder
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Newobj, commandBuilderField.FieldType.GetConstructor(Type.EmptyTypes));
            _moveNextMethodIL.Emit(OpCodes.Stfld, commandBuilderField);

            for (var entityIndex = 0; entityIndex < entities.Length; entityIndex++)
            {
                var entityHolder = entities[entityIndex];
                var entity = entityHolder.Entity;

                var stringBuilder = new StringBuilder();

                var skipPrimaryKey = entity.HasDbGeneratedPrimaryKey;

                var totalColumnCount = entity.GetColumnCount();
                var columnCount = totalColumnCount - entity.GetReadOnlyCount();
                var columnOffset = skipPrimaryKey ? entity.GetRegularColumnOffset() : 0;
                var lastNonReadOnlyIndex = entity.GetLastRegularColumnsIndex();

                stringBuilder.Append("INSERT INTO ")
                             .Append(entity.TableName)
                             .Append(" (")
                             .Append(skipPrimaryKey ? entity.NonPrimaryColumnListString : entity.ColumnListString)
                             .Append(") VALUES ");

                if (entity == _rootEntity)
                {
                    stringBuilder.Append('(');

                    for (var columnIndex = columnOffset; columnIndex <= lastNonReadOnlyIndex; columnIndex++)
                    {
                        var column = _rootEntity.GetColumn(columnIndex);

                        if (column.Options.HasFlag(ColumnOptions.ReadOnly))
                            continue;

                        if (column.Options.HasFlag(ColumnOptions.DefaultValue))
                        {
                            stringBuilder.Append("DEFAULT, ");
                        }
                        else
                        {
                            stringBuilder.Append('@')
                                         .Append(column.PropertyInfo.Name)
                                         .Append(", ");
                        }
                    }

                    stringBuilder.Length -= 2;

                    if (skipPrimaryKey)
                    {
                        stringBuilder.Append(") RETURNING ")
                                     .Append(_rootEntity.GetPrimaryColumn().NormalizedColumnName)
                                     .Append(";");
                    }
                    else
                    {
                        stringBuilder.Append(");");
                    }

                    // Clear commandBuilder and command parameters
                    if (entityIndex > 0)
                    {
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Clear"));
                        _moveNextMethodIL.Emit(OpCodes.Pop);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).PropertyType.GetMethod("Clear"));
                    }

                    // Assign the commandBuilder text to the command
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, stringBuilder.ToString());
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("CommandText").GetSetMethod());

                    // assign foreign key to itself from navigation property primary key
                    for (var relationIndex = entityHolder.SelfAssignedRelations.Count - 1; relationIndex >= 0; relationIndex--)
                    {
                        var relation = entityHolder.SelfAssignedRelations[relationIndex];

                        // Check if navigation property is null

                        var afterBodyLabel = _moveNextMethodIL.DefineLabel();

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Brfalse, afterBodyLabel);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                        WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);

                        _moveNextMethodIL.MarkLabel(afterBodyLabel);
                    }

                    // Assign parameters to command
                    for (var columnIndex = columnOffset; columnIndex <= lastNonReadOnlyIndex; columnIndex++)
                    {
                        var column = _rootEntity.GetColumn(columnIndex);

                        if (column.Options.HasFlag(ColumnOptions.ReadOnly) ||
                            column.Options.HasFlag(ColumnOptions.DefaultValue))
                            continue;

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                        WriteNpgsqlParameterFromColumn(_moveNextMethodIL, _rootEntityInsertField, column);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameterCollection).GetMethod("Add", new[] { typeof(NpgsqlParameter) }));
                        _moveNextMethodIL.Emit(OpCodes.Pop);
                    }

                    // Get the result of the command
                    if (skipPrimaryKey)
                    {
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteScalarAsync", new[] { _cancellationTokenField.FieldType }));

                        asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<object>), _moveNextMethodIL.DeclareLocal(typeof(TaskAwaiter<object>)), _stateMachineTypeBuilder.DefineField("_objectTaskAwaiter", typeof(TaskAwaiter<object>), FieldAttributes.Private));

                        var objectResultLocal = _moveNextMethodIL.DeclareLocal(typeof(object));
                        _moveNextMethodIL.Emit(OpCodes.Stloc, objectResultLocal);

                        // cast and store primary key
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, objectResultLocal);
                        _moveNextMethodIL.Emit(OpCodes.Unbox_Any, _rootEntity.GetPrimaryColumn().PropertyInfo.PropertyType);

                        WritePropertyAssigner(_moveNextMethodIL, _rootEntity.GetPrimaryColumn());
                    }
                    else
                    {
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteNonQueryAsync", new[] { _cancellationTokenField.FieldType }));

                        intTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_intTaskAwaiter", typeof(TaskAwaiter<int>), FieldAttributes.Private);
                        intTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(intTaskAwaiterField.FieldType);

                        asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<int>), intTaskAwaiterLocal, intTaskAwaiterField);

                        _moveNextMethodIL.Emit(OpCodes.Pop);
                    }

                    if (_loggerField is not null)
                    {
                        // log success of insert
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _loggerField);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)CommandType.InsertSingle);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _loggerField.FieldType.GetMethod("Invoke"));
                    }

                    if (entityHolder.ForeignAssignedRelations.Count > 0)
                    {
                        LocalBuilder? innerIteratorLocal = default;

                        for (var relationIndex = entityHolder.ForeignAssignedRelations.Count - 1; relationIndex >= 0; relationIndex--)
                        {
                            var relation = entityHolder.ForeignAssignedRelations[relationIndex];

                            var afterOuterNullCheckBodyLabel = _moveNextMethodIL.DefineLabel();

                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Brfalse, afterOuterNullCheckBodyLabel);

                            if (relation.RelationType == RelationType.OneToMany)
                            {
                                innerIteratorLocal ??= _moveNextMethodIL.DeclareLocal(_intType);

                                var innerLoopConditionLabel = _moveNextMethodIL.DefineLabel();
                                var innertStartLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                                var foreignEntityLocal = _moveNextMethodIL.DeclareLocal(relation.RightEntity.EntityType);

                                // Assign 0 to the iterator
                                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                                _moveNextMethodIL.Emit(OpCodes.Stloc, innerIteratorLocal);
                                _moveNextMethodIL.Emit(OpCodes.Br, innerLoopConditionLabel);

                                // loop body
                                _moveNextMethodIL.MarkLabel(innertStartLoopBodyLabel);

                                // assign iterator element to foreignEntityLocal
                                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                                _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetMethod("get_Item"));
                                _moveNextMethodIL.Emit(OpCodes.Stloc, foreignEntityLocal);

                                // check if element is not null
                                var afterNullCheckBodyLabel = _moveNextMethodIL.DefineLabel();

                                _moveNextMethodIL.Emit(OpCodes.Ldloc, foreignEntityLocal);
                                _moveNextMethodIL.Emit(OpCodes.Brfalse, afterNullCheckBodyLabel);

                                // assign entity primary key to foreign key on navigation entity
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, foreignEntityLocal);
                                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                                _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                                WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);

                                _moveNextMethodIL.MarkLabel(afterNullCheckBodyLabel);

                                // loop iterator increment
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                                _moveNextMethodIL.Emit(OpCodes.Add);
                                _moveNextMethodIL.Emit(OpCodes.Stloc, innerIteratorLocal);

                                // loop condition
                                _moveNextMethodIL.MarkLabel(innerLoopConditionLabel);
                                _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                                _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Blt, innertStartLoopBodyLabel);
                            }
                            else
                            {
                                // assign entity primary key to foreign key on navigation entity
                                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                                _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                                _moveNextMethodIL.Emit(OpCodes.Ldfld, _rootEntityInsertField);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, _rootEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                                WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);
                            }

                            _moveNextMethodIL.MarkLabel(afterOuterNullCheckBodyLabel);
                        }
                    }
                }
                else
                {
                    var entityId = _reachableEntities.HasId(entity, out _);

                    var entityCollection = entityCollections[entityId];

                    // Check if entityCollection is larger than 0
                    var endOfEntityInsertLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Brfalse, endOfEntityInsertLabel);

                    // Clear commandBuilder and command parameters
                    if (entityIndex > 0)
                    {
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Clear"));
                        _moveNextMethodIL.Emit(OpCodes.Pop);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).PropertyType.GetMethod("Clear"));
                    }

                    // Outer loop to keep one single command under ushort.MaxValue parameters

                    var totalLocal = _moveNextMethodIL.DeclareLocal(_intType);
                    var currentLocal = _moveNextMethodIL.DeclareLocal(_intType);

                    // Assign the total amount of parameters to the total local
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Stloc, totalLocal);

                    // Assign 0 to the current local
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, currentLocal);

                    var outerIteratorLocal = _moveNextMethodIL.DeclareLocal(_intType);

                    var outerLoopConditionLabel = _moveNextMethodIL.DefineLabel();
                    var outerStartLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                    // Assign 0 to the iterator
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, outerIteratorLocal);
                    _moveNextMethodIL.Emit(OpCodes.Br, outerLoopConditionLabel);

                    // loop body
                    _moveNextMethodIL.MarkLabel(outerStartLoopBodyLabel);

                    // Append base Insert Command to command builder
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, stringBuilder.ToString());
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    var leftLocal = _moveNextMethodIL.DeclareLocal(_intType);

                    var totalColumns = columnCount - columnOffset;

                    // Assign the amount of left items to the left local
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, totalLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
                    _moveNextMethodIL.Emit(OpCodes.Mul);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
                    _moveNextMethodIL.Emit(OpCodes.Mul);
                    _moveNextMethodIL.Emit(OpCodes.Sub);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, ushort.MaxValue);
                    _moveNextMethodIL.Emit(OpCodes.Call, typeof(Math).GetMethod("Min", new[] { _intType, _intType }));
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, totalColumns);
                    _moveNextMethodIL.Emit(OpCodes.Div);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                    _moveNextMethodIL.Emit(OpCodes.Add);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, leftLocal);

                    var iteratorElementLocal = _moveNextMethodIL.DeclareLocal(entity.EntityType);

                    var loopConditionLabel = _moveNextMethodIL.DefineLabel();
                    var startLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                    // loop body
                    _moveNextMethodIL.MarkLabel(startLoopBodyLabel);

                    // get element at iterator from list
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.GetMethod("get_Item"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, iteratorElementLocal);

                    // assign foreign key to itself from navigation property primary key
                    for (var relationIndex = entityHolder.SelfAssignedRelations.Count - 1; relationIndex >= 0; relationIndex--)
                    {
                        var relation = entityHolder.SelfAssignedRelations[relationIndex];

                        // Check if navigation property is null

                        var afterBodyLabel = _moveNextMethodIL.DefineLabel();

                        _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Brfalse, afterBodyLabel);

                        _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                        WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);

                        _moveNextMethodIL.MarkLabel(afterBodyLabel);
                    }

                    // append placeholders to command builder
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)'(');
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(char) }));
                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    for (var k = columnOffset; k <= lastNonReadOnlyIndex; k++)
                    {
                        var column = entity.GetColumn(k);

                        if (column.Options.HasFlag(ColumnOptions.ReadOnly))
                            continue;

                        // Write placeholder to the command builder => (@Name(n)),
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);

                        if (column.Options.HasFlag(ColumnOptions.DefaultValue))
                        {
                            _moveNextMethodIL.Emit(OpCodes.Ldstr, lastNonReadOnlyIndex == k ? "DEFAULT), " : "DEFAULT, ");
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                            _moveNextMethodIL.Emit(OpCodes.Pop);
                        }
                        else
                        {
                            // Create new parameter with placeholder and add it to the parameter list
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());

                            WriteNpgsqlParameterFromColumn(_moveNextMethodIL, iteratorElementLocal, column, currentLocal);

                            _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameterCollection).GetMethod("Add", new[] { typeof(NpgsqlParameter) }));

                            // Write placeholder to the command builder => (@Name(n)),
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameter).GetProperty("ParameterName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                            _moveNextMethodIL.Emit(OpCodes.Ldstr, lastNonReadOnlyIndex == k ? "), " : ", ");
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                            _moveNextMethodIL.Emit(OpCodes.Pop);
                        }
                    }

                    // loop iterator increment
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    _moveNextMethodIL.Emit(OpCodes.Add);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, currentLocal);

                    // loop condition
                    _moveNextMethodIL.MarkLabel(loopConditionLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, leftLocal);
                    _moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

                    // Remove the last the values form the command string e.g. ", "
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetProperty("Length").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_2);
                    _moveNextMethodIL.Emit(OpCodes.Sub);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetProperty("Length").GetSetMethod());

                    if (skipPrimaryKey)
                    {
                        // Append " RETURNING \"PrimaryKey\""
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                        _moveNextMethodIL.Emit(OpCodes.Ldstr, " RETURNING " + entity.GetPrimaryColumn().NormalizedColumnName + ";");
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                        _moveNextMethodIL.Emit(OpCodes.Pop);
                    }

                    // outer loop iterator increment
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, outerIteratorLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    _moveNextMethodIL.Emit(OpCodes.Add);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, outerIteratorLocal);

                    // outer loop condition
                    _moveNextMethodIL.MarkLabel(outerLoopConditionLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, totalLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                    _moveNextMethodIL.Emit(OpCodes.Bne_Un, outerStartLoopBodyLabel);

                    // Assign the commandBuilder text to the command
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);

                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("ToString", Type.EmptyTypes));
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetProperty("CommandText").GetSetMethod());

                    if (skipPrimaryKey)
                    {
                        dataReaderField ??= _stateMachineTypeBuilder.DefineField("_dataReader", typeof(NpgsqlDataReader), FieldAttributes.Private);

                        // Get the result of the command
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteReaderAsync", new[] { _cancellationTokenField.FieldType }));

                        dataReaderTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_dataReaderTaskAwaiter", typeof(TaskAwaiter<NpgsqlDataReader>), FieldAttributes.Private);
                        dataReaderTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(dataReaderTaskAwaiterField.FieldType);

                        asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<NpgsqlDataReader>), dataReaderTaskAwaiterLocal, dataReaderTaskAwaiterField);

                        var dataReaderLocal = _moveNextMethodIL.DeclareLocal(dataReaderField.FieldType);

                        _moveNextMethodIL.Emit(OpCodes.Stloc, dataReaderLocal);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, dataReaderLocal);
                        _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);

                        iteratorField ??= _stateMachineTypeBuilder.DefineField("_iterator", _intType, FieldAttributes.Private);
                        counterField ??= _stateMachineTypeBuilder.DefineField("_counter", typeof(ushort), FieldAttributes.Private);

                        loopConditionLabel = _moveNextMethodIL.DefineLabel();
                        startLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                        // Assign 0 to the counter
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                        _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                        // Assign 0 to the iterator
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                        _moveNextMethodIL.Emit(OpCodes.Stfld, iteratorField);
                        _moveNextMethodIL.Emit(OpCodes.Br, loopConditionLabel);

                        // loop body
                        _moveNextMethodIL.MarkLabel(startLoopBodyLabel);

                        // check if counter is equal to ushort.MaxValue

                        var afterIfBody = _moveNextMethodIL.DefineLabel();

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, counterField);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4, ushort.MaxValue / columnCount);
                        _moveNextMethodIL.Emit(OpCodes.Bne_Un, afterIfBody);

                        // Assign 0 to the counter
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                        _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                        // Call the next result
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("NextResultAsync", new[] { _cancellationTokenField.FieldType }));

                        boolTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_boolTaskAwaiter", typeof(TaskAwaiter<bool>), FieldAttributes.Private);
                        boolTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(boolTaskAwaiterField.FieldType);

                        asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), boolTaskAwaiterLocal, boolTaskAwaiterField);

                        _moveNextMethodIL.Emit(OpCodes.Pop);

                        _moveNextMethodIL.MarkLabel(afterIfBody);

                        // read data reader

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("ReadAsync", new[] { _cancellationTokenField.FieldType }));

                        asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<bool>), boolTaskAwaiterLocal, boolTaskAwaiterField);

                        _moveNextMethodIL.Emit(OpCodes.Pop);

                        // assign foreign key to itself from navigation property primary key
                        if (entityHolder.ForeignAssignedRelations.Count > 0)
                        {
                            var primaryKeyLocal = _moveNextMethodIL.DeclareLocal(entity.GetPrimaryColumn().PropertyInfo.PropertyType);

                            // assign the returned id to the local
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("GetFieldValue").MakeGenericMethod(entity.GetPrimaryColumn().PropertyInfo.PropertyType));
                            _moveNextMethodIL.Emit(OpCodes.Stloc, primaryKeyLocal);

                            var entityLocal = _moveNextMethodIL.DeclareLocal(entity.EntityType);

                            // assign the current entity to the local
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.GetMethod("get_Item"));
                            _moveNextMethodIL.Emit(OpCodes.Stloc, entityLocal);

                            // assign the returned id to the current entity
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);

                            WritePropertyAssigner(_moveNextMethodIL, entity.GetPrimaryColumn());

                            LocalBuilder? innerIteratorLocal = default;

                            for (var relationIndex = entityHolder.ForeignAssignedRelations.Count - 1; relationIndex >= 0; relationIndex--)
                            {
                                var relation = entityHolder.ForeignAssignedRelations[relationIndex];

                                var afterOuterNullCheckBodyLabel = _moveNextMethodIL.DefineLabel();

                                _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                                _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                _moveNextMethodIL.Emit(OpCodes.Brfalse, afterOuterNullCheckBodyLabel);

                                if (relation.RelationType == RelationType.OneToMany)
                                {
                                    innerIteratorLocal ??= _moveNextMethodIL.DeclareLocal(_intType);

                                    var innerLoopConditionLabel = _moveNextMethodIL.DefineLabel();
                                    var innertStartLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                                    var foreignEntityLocal = _moveNextMethodIL.DeclareLocal(relation.RightEntity.EntityType);

                                    // Assign 0 to the iterator
                                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                                    _moveNextMethodIL.Emit(OpCodes.Stloc, innerIteratorLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Br, innerLoopConditionLabel);

                                    // loop body
                                    _moveNextMethodIL.MarkLabel(innertStartLoopBodyLabel);

                                    // assign iterator element to foreignEntityLocal
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetMethod("get_Item"));
                                    _moveNextMethodIL.Emit(OpCodes.Stloc, foreignEntityLocal);

                                    // check if element is not null
                                    var afterNullCheckBodyLabel = _moveNextMethodIL.DefineLabel();

                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, foreignEntityLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Brfalse, afterNullCheckBodyLabel);

                                    // assign entity primary key to foreign key on navigation entity
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, foreignEntityLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                                    WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);

                                    _moveNextMethodIL.MarkLabel(afterNullCheckBodyLabel);

                                    // loop iterator increment
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                                    _moveNextMethodIL.Emit(OpCodes.Add);
                                    _moveNextMethodIL.Emit(OpCodes.Stloc, innerIteratorLocal);

                                    // loop condition
                                    _moveNextMethodIL.MarkLabel(innerLoopConditionLabel);
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, innerIteratorLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                    _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                                    _moveNextMethodIL.Emit(OpCodes.Blt, innertStartLoopBodyLabel);
                                }
                                else
                                {
                                    // assign entity primary key to foreign key on navigation entity
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, entityLocal);
                                    _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                                    _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                                    WritePropertyAssigner(_moveNextMethodIL, relation.ForeignKeyColumn);
                                }

                                _moveNextMethodIL.MarkLabel(afterOuterNullCheckBodyLabel);
                            }
                        }
                        else
                        {
                            // assign the returned id to the current element
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.GetMethod("get_Item"));
                            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                            _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("GetFieldValue").MakeGenericMethod(entity.GetPrimaryColumn().PropertyInfo.PropertyType));
                            WritePropertyAssigner(_moveNextMethodIL, entity.GetPrimaryColumn());
                        }

                        // loop iterator increment
                        var tempIteratorLocal = _moveNextMethodIL.DeclareLocal(_intType);
                        var tempCounterLocal = _moveNextMethodIL.DeclareLocal(_intType);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                        _moveNextMethodIL.Emit(OpCodes.Stloc, tempIteratorLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, tempIteratorLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                        _moveNextMethodIL.Emit(OpCodes.Add);
                        _moveNextMethodIL.Emit(OpCodes.Stfld, iteratorField);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, counterField);
                        _moveNextMethodIL.Emit(OpCodes.Stloc, tempCounterLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, tempCounterLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                        _moveNextMethodIL.Emit(OpCodes.Add);
                        _moveNextMethodIL.Emit(OpCodes.Stfld, counterField);

                        // loop condition
                        _moveNextMethodIL.MarkLabel(loopConditionLabel);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

                        // dispose data reader
                        valueTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_valueTaskAwaiter", typeof(ValueTaskAwaiter), FieldAttributes.Private);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("DisposeAsync"));

                        asyncGenerator.WriteAsyncValueTaskMethodAwaiter(_moveNextMethodIL.DeclareLocal(typeof(ValueTask)), valueTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(valueTaskAwaiterField.FieldType), valueTaskAwaiterField);

                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldnull);
                        _moveNextMethodIL.Emit(OpCodes.Stfld, dataReaderField);
                    }
                    else
                    {
                        // Get the result of the command
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _commandField);
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _cancellationTokenField);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _commandField.FieldType.GetMethod("ExecuteNonQueryAsync", new[] { _cancellationTokenField.FieldType }));

                        intTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_intTaskAwaiter", typeof(TaskAwaiter<int>), FieldAttributes.Private);
                        intTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(intTaskAwaiterField.FieldType);

                        asyncGenerator.WriteAsyncMethodAwaiter(typeof(Task<int>), intTaskAwaiterLocal, intTaskAwaiterField);


                        _moveNextMethodIL.Emit(OpCodes.Pop);
                    }

                    if (_loggerField is not null)
                    {
                        // log success of insert
                        _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                        _moveNextMethodIL.Emit(OpCodes.Ldfld, _loggerField);
                        _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)CommandType.InsertSingle);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, _loggerField.FieldType.GetMethod("Invoke"));
                    }

                    _moveNextMethodIL.MarkLabel(endOfEntityInsertLabel);
                }
            }

            // return the amount of inserted rows
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);

            foreach (var entityCollection in entityCollections.Values)
            {
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityCollection);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityCollection.FieldType.FindProperty("Count", _genericICollectionType).GetGetMethod());
                _moveNextMethodIL.Emit(OpCodes.Add);
            }

            _moveNextMethodIL.Emit(OpCodes.Stloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            var exceptionLocal = _moveNextMethodIL.DeclareLocal(typeof(Exception));

            _moveNextMethodIL.BeginCatchBlock(exceptionLocal.LocalType);
            // Set state and return exception
            _moveNextMethodIL.Emit(OpCodes.Stloc, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

            // End of catch block
            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfMethodLabel);

            // Set state and return result
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, _stateField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, _methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, insertedCountLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("SetResult"));

            // End of method
            _moveNextMethodIL.MarkLabel(retOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);
        }

        private void CreateSingleNoRelationNoDbKeysInserter(ILGenerator iLGenerator)
        {
            // Check if insert is null
            var afterRootInsertNullCheck = iLGenerator.DefineLabel();

            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Brtrue, afterRootInsertNullCheck);

            // Return from method and assign -1 to the insert count
            iLGenerator.Emit(OpCodes.Ldc_I4_M1);
            iLGenerator.Emit(OpCodes.Call, typeof(Task).GetMethod("FromResult", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(_intType));
            iLGenerator.Emit(OpCodes.Ret);

            iLGenerator.MarkLabel(afterRootInsertNullCheck);

            // Create command and assign sql command and connection
            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append("INSERT INTO ")
                      .Append(_rootEntity.TableName)
                      .Append(" (")
                      .Append(_rootEntity.ColumnListString)
                      .Append(") VALUES (");

            var lastNonReadOnlyIndex = _rootEntity.GetLastRegularColumnsIndex();

            for (var columnIndex = 0; columnIndex <= lastNonReadOnlyIndex; columnIndex++)
            {
                var column = _rootEntity.GetColumn(columnIndex);

                if (column.Options.HasFlag(ColumnOptions.ReadOnly))
                    continue;

                if (column.Options.HasFlag(ColumnOptions.DefaultValue))
                {
                    sqlBuilder.Append("DEFAULT, ");
                }
                else
                {
                    sqlBuilder.Append('@')
                              .Append(column.PropertyInfo.Name)
                              .Append(", ");
                }
            }

            sqlBuilder.Length -= 2;

            sqlBuilder.Append(");");

            var rootEntityLocal = iLGenerator.DeclareLocal(_rootEntity.EntityType);

            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Stloc, rootEntityLocal);

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Dup);
            iLGenerator.Emit(OpCodes.Ldstr, sqlBuilder.ToString());
            iLGenerator.Emit(OpCodes.Callvirt, typeof(NpgsqlCommand).GetProperty("CommandText").GetSetMethod());

            // Assign parameters to command
            for (var columnIndex = 0; columnIndex <= lastNonReadOnlyIndex; columnIndex++)
            {
                var column = _rootEntity.GetColumn(columnIndex);

                if (column.Options.HasFlag(ColumnOptions.ReadOnly) ||
                    column.Options.HasFlag(ColumnOptions.DefaultValue))
                    continue;

                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Callvirt, typeof(NpgsqlCommand).GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                WriteNpgsqlParameterFromColumn(iLGenerator, rootEntityLocal, column);
                iLGenerator.Emit(OpCodes.Callvirt, typeof(NpgsqlParameterCollection).GetMethod("Add", new[] { typeof(NpgsqlParameter) }));
                iLGenerator.Emit(OpCodes.Pop);
            }

            // Get the result of the command
            iLGenerator.Emit(OpCodes.Ldarg_2);
            iLGenerator.Emit(OpCodes.Callvirt, typeof(NpgsqlCommand).GetMethod("ExecuteNonQueryAsync", new[] { typeof(CancellationToken) }));

            // End of method
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void WritePropertyAssigner(ILGenerator ilGenerator, EntityColumn column)
        {
            var underlyingType = Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType);

            if (underlyingType is not null)
            {
                ilGenerator.Emit(OpCodes.Newobj, column.PropertyInfo.PropertyType.GetConstructor(new[] { underlyingType }));
            }

            if (column.Options.HasFlag(ColumnOptions.ReadOnly))
                ilGenerator.Emit(OpCodes.Stfld, column.PropertyInfo.GetBackingField());
            else
                ilGenerator.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod(true));
        }

        private void WriteNpgsqlParameterFromColumn(ILGenerator ilGenerator, object entityVariable, EntityColumn column, LocalBuilder? iteratorLocal = default)
        {
            var underlyingType = Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType);

            var lb = entityVariable as LocalBuilder;
            var fb = lb is null ? entityVariable as FieldBuilder : default;

            if (lb is null &&
                fb is null)
            {
                throw new ArgumentException("The parameter has to be either of type 'FieldBuilder' or 'LocalBuilder'.", nameof(entityVariable));
            }

            var stringType = typeof(string);

            if (underlyingType is not null)
            {
                var dbNullType = typeof(DBNull);

                var propertyLocal = ilGenerator.DeclareLocal(column.PropertyInfo.PropertyType);

                var defaultRetrieverLabel = ilGenerator.DefineLabel();
                var afterHasValueLabel = ilGenerator.DefineLabel();

                // Check if property has value
                if (fb is not null)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldfld, fb);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldloc, lb);
                }

                ilGenerator.Emit(OpCodes.Callvirt, column.PropertyInfo.GetGetMethod());
                ilGenerator.Emit(OpCodes.Stloc, propertyLocal);
                ilGenerator.Emit(OpCodes.Ldloca, propertyLocal);
                ilGenerator.Emit(OpCodes.Call, propertyLocal.LocalType.GetProperty("HasValue").GetGetMethod());
                ilGenerator.Emit(OpCodes.Brtrue, defaultRetrieverLabel);

                // Nullable retriever
                ilGenerator.Emit(OpCodes.Ldstr, '@' + column.PropertyInfo.Name);

                if (iteratorLocal is not null)
                {
                    ilGenerator.Emit(OpCodes.Ldloca, iteratorLocal);
                    ilGenerator.Emit(OpCodes.Call, iteratorLocal.LocalType.GetMethod("ToString", Type.EmptyTypes));
                    ilGenerator.Emit(OpCodes.Call, stringType.GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { stringType, stringType }, null));
                }

                ilGenerator.Emit(OpCodes.Ldsfld, dbNullType.GetField("Value"));
                ilGenerator.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(dbNullType).GetConstructor(new[] { stringType, dbNullType }));
                ilGenerator.Emit(OpCodes.Br, afterHasValueLabel);

                // Default retriever
                ilGenerator.MarkLabel(defaultRetrieverLabel);

                ilGenerator.Emit(OpCodes.Ldstr, '@' + column.PropertyInfo.Name);

                if (iteratorLocal is not null)
                {
                    ilGenerator.Emit(OpCodes.Ldloca, iteratorLocal);
                    ilGenerator.Emit(OpCodes.Call, iteratorLocal.LocalType.GetMethod("ToString", Type.EmptyTypes));
                    ilGenerator.Emit(OpCodes.Call, stringType.GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { stringType, stringType }, null));
                }

                if (fb is not null)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldfld, fb);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldloc, lb);
                }

                ilGenerator.Emit(OpCodes.Callvirt, column.PropertyInfo.GetGetMethod());
                ilGenerator.Emit(OpCodes.Stloc, propertyLocal);
                ilGenerator.Emit(OpCodes.Ldloca, propertyLocal);
                ilGenerator.Emit(OpCodes.Call, propertyLocal.LocalType.GetProperty("Value").GetGetMethod());

                if (underlyingType.IsEnum &&
                    !column.Options.HasFlag(ColumnOptions.PostgreEnum))
                {
                    underlyingType = Enum.GetUnderlyingType(underlyingType);
                }
                else if (typeof(IKey).IsAssignableFrom(underlyingType))
                {
                    var keyLocal = ilGenerator.DeclareLocal(underlyingType);

                    var underlyingStronglyTypedKeyType = underlyingType.GetInterface(typeof(IKey<,>).Name).GetGenericArguments()[1];

                    ilGenerator.Emit(OpCodes.Stloc, keyLocal);
                    ilGenerator.Emit(OpCodes.Ldloca, keyLocal);

                    ilGenerator.Emit(OpCodes.Call, underlyingType.GetCastMethod(underlyingType, underlyingStronglyTypedKeyType));

                    underlyingType = underlyingStronglyTypedKeyType;
                }

                if (underlyingType == typeof(ulong))
                {
                    underlyingType = typeof(long);

                    ilGenerator.Emit(OpCodes.Ldc_I8, long.MinValue);
                    ilGenerator.Emit(OpCodes.Add);
                }

                if (column.DbType is null)
                {
                    ilGenerator.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(underlyingType).GetConstructor(new[] { stringType, underlyingType }));
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldc_I4, (int)column.DbType);
                    ilGenerator.Emit(OpCodes.Call, typeof(NpgsqlParameterExtensions).GetMethod("CreateParameter", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(underlyingType));
                }

                ilGenerator.MarkLabel(afterHasValueLabel);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldstr, '@' + column.PropertyInfo.Name);

                if (iteratorLocal is not null)
                {
                    ilGenerator.Emit(OpCodes.Ldloca, iteratorLocal);
                    ilGenerator.Emit(OpCodes.Call, iteratorLocal.LocalType.GetMethod("ToString", Type.EmptyTypes));
                    ilGenerator.Emit(OpCodes.Call, stringType.GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { stringType, stringType }, null));
                }

                if (fb is not null)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldfld, fb);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldloc, lb);
                }

                ilGenerator.Emit(OpCodes.Callvirt, column.PropertyInfo.GetGetMethod());

                Type npgsqlType;

                var isUlong = column.PropertyInfo.PropertyType == typeof(ulong);

                if (column.PropertyInfo.PropertyType.IsEnum &&
                    !column.Options.HasFlag(ColumnOptions.PostgreEnum))
                {
                    npgsqlType = Enum.GetUnderlyingType(column.PropertyInfo.PropertyType);
                }
                else if (typeof(IKey).IsAssignableFrom(column.PropertyInfo.PropertyType))
                {
                    var keyLocal = ilGenerator.DeclareLocal(column.PropertyInfo.PropertyType);

                    npgsqlType = column.PropertyInfo.PropertyType.GetInterface(typeof(IKey<,>).Name).GetGenericArguments()[1];

                    ilGenerator.Emit(OpCodes.Stloc, keyLocal);
                    ilGenerator.Emit(OpCodes.Ldloca, keyLocal);

                    ilGenerator.Emit(OpCodes.Call, column.PropertyInfo.PropertyType.GetCastMethod(column.PropertyInfo.PropertyType, npgsqlType));

                    if (npgsqlType == typeof(ulong))
                        isUlong = true;
                }
                else
                {
                    npgsqlType = column.PropertyInfo.PropertyType;
                }

                if (isUlong)
                {
                    npgsqlType = typeof(long);

                    ilGenerator.Emit(OpCodes.Ldc_I8, long.MinValue);
                    ilGenerator.Emit(OpCodes.Add);
                }

                if (column.DbType is null)
                {
                    ilGenerator.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(npgsqlType).GetConstructor(new[] { stringType, npgsqlType }));
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldc_I4, (int)column.DbType);
                    ilGenerator.Emit(OpCodes.Call, typeof(NpgsqlParameterExtensions).GetMethod("CreateParameter", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(npgsqlType));
                }
            }
        }

        private class EntitySeprator
        {
            private LocalBuilder _entityIdCheckerLocal = null!;
            private LocalBuilder _firstTimeLocal = null!;

            private readonly ILGenerator _ilGenerator;
            private readonly TypeBuilder _typeBuilder;
            private readonly Entity _rootEntity;
            private readonly EntityRelationHolder[] _entities;
            private readonly ObjectIDGenerator _reachableEntities;
            private readonly HashSet<uint> _reachableRelations;

            private readonly HashSet<long> _vistiedEntities;
            private readonly HashSet<uint> _visitedRelations;
            private readonly Dictionary<long, FieldBuilder> _entityCollections;
            private readonly Dictionary<long, EntityRelationHolder> _entityHolders;

            internal EntitySeprator(ILGenerator ilGenerator, TypeBuilder typeBuilder, Entity rootEntity, EntityRelationHolder[] entities, ObjectIDGenerator reachableEntities, HashSet<uint> reachableRelations)
            {
                _ilGenerator = ilGenerator;
                _typeBuilder = typeBuilder;
                _rootEntity = rootEntity;
                _entities = entities;
                _reachableEntities = reachableEntities;
                _reachableRelations = reachableRelations;

                _vistiedEntities = new HashSet<long>();
                _visitedRelations = new HashSet<uint>();
                _entityCollections = new Dictionary<long, FieldBuilder>();

                _entityHolders = new Dictionary<long, EntityRelationHolder>(entities.Length);

                for (var i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];

                    _entityHolders.Add(_reachableEntities.HasId(entity.Entity, out _), entity);
                }
            }

            internal Dictionary<long, FieldBuilder> WriteEntitySeperater(FieldBuilder entityInsertField)
            {
                if (_rootEntity.Relations is null)
                    return _entityCollections;

                WriteEntitySetup();

                var startLoopBodyLabel = _ilGenerator.DefineLabel();
                var loopConditionLabel = _ilGenerator.DefineLabel();

                var iteratorLocal = _ilGenerator.DeclareLocal(typeof(int));
                var iteratorElementLocal = _ilGenerator.DeclareLocal(_rootEntity.EntityType);

                // Assign 0 to the iterator
                _ilGenerator.Emit(OpCodes.Ldc_I4_0);
                _ilGenerator.Emit(OpCodes.Stloc, iteratorLocal);
                _ilGenerator.Emit(OpCodes.Br, loopConditionLabel);

                // loop body
                _ilGenerator.MarkLabel(startLoopBodyLabel);

                // iterator
                _ilGenerator.Emit(OpCodes.Ldarg_0);
                _ilGenerator.Emit(OpCodes.Ldfld, entityInsertField);
                _ilGenerator.Emit(OpCodes.Ldloc, iteratorLocal);
                _ilGenerator.Emit(OpCodes.Callvirt, entityInsertField.FieldType.GetMethod("get_Item"));
                _ilGenerator.Emit(OpCodes.Stloc, iteratorElementLocal);

                WriteEntitySeperaterBase(_rootEntity, iteratorElementLocal, null, null);

                // loop iterator increment
                _ilGenerator.Emit(OpCodes.Ldloc, iteratorLocal);
                _ilGenerator.Emit(OpCodes.Ldc_I4_1);
                _ilGenerator.Emit(OpCodes.Add);
                _ilGenerator.Emit(OpCodes.Stloc, iteratorLocal);

                // loop condition
                _ilGenerator.MarkLabel(loopConditionLabel);
                _ilGenerator.Emit(OpCodes.Ldloc, iteratorLocal);
                _ilGenerator.Emit(OpCodes.Ldarg_0);
                _ilGenerator.Emit(OpCodes.Ldfld, entityInsertField);
                _ilGenerator.Emit(OpCodes.Callvirt, entityInsertField.FieldType.FindProperty("Count", typeof(ICollection<>)).GetGetMethod());
                _ilGenerator.Emit(OpCodes.Blt, startLoopBodyLabel);

                return _entityCollections;
            }

            internal Dictionary<long, FieldBuilder> WriteFlatEntitySeperater(FieldBuilder entityInsertField)
            {
                if (_rootEntity.Relations is null)
                    return _entityCollections;

                WriteEntitySetup();

                var entityInsertLocal = _ilGenerator.DeclareLocal(entityInsertField.FieldType);

                _ilGenerator.Emit(OpCodes.Ldarg_0);
                _ilGenerator.Emit(OpCodes.Ldfld, entityInsertField);
                _ilGenerator.Emit(OpCodes.Stloc, entityInsertLocal);

                WriteEntitySeperaterBase(_rootEntity, entityInsertLocal, null, null);

                return _entityCollections;
            }

            private void WriteEntitySetup()
            {
                // instantiate entityCheckerLocal
                _entityIdCheckerLocal = _ilGenerator.DeclareLocal(typeof(ObjectIDGenerator));

                _ilGenerator.Emit(OpCodes.Newobj, _entityIdCheckerLocal.LocalType.GetConstructor(Type.EmptyTypes));
                _ilGenerator.Emit(OpCodes.Stloc, _entityIdCheckerLocal);

                // instantiate entityCollections
                for (var entityIndex = _entities.Length - 1; entityIndex >= 0; entityIndex--)
                {
                    var entity = _entities[entityIndex].Entity;

                    if (entity == _rootEntity)
                        continue;

                    var entityId = _reachableEntities.HasId(entity, out _);

                    var entityCollectionField = _typeBuilder.DefineField("_" + entity.EntityName + "Collection", typeof(List<>).MakeGenericType(entity.EntityType), FieldAttributes.Private);

                    _entityCollections.Add(entityId, entityCollectionField);

                    _ilGenerator.Emit(OpCodes.Ldarg_0);
                    _ilGenerator.Emit(OpCodes.Newobj, entityCollectionField.FieldType.GetConstructor(Type.EmptyTypes));
                    _ilGenerator.Emit(OpCodes.Stfld, entityCollectionField);
                }

                _firstTimeLocal = _ilGenerator.DeclareLocal(typeof(bool));
            }

            private void WriteEntitySeperaterBase(Entity entity, LocalBuilder leftEntityLocal, Entity? lastEntity, LocalBuilder? lastEntityLocal)
            {
                var entityId = _reachableEntities.HasId(entity, out _);

                _vistiedEntities.Add(entityId);

                // Check if element is not null
                var afterSplitLabel = _ilGenerator.DefineLabel();

                _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                _ilGenerator.Emit(OpCodes.Brfalse, afterSplitLabel);

                // Check if element has been visited before
                _ilGenerator.Emit(OpCodes.Ldloc, _entityIdCheckerLocal);
                _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                _ilGenerator.Emit(OpCodes.Ldloca, _firstTimeLocal);
                _ilGenerator.Emit(OpCodes.Callvirt, _entityIdCheckerLocal.LocalType.GetMethod("GetId"));
                _ilGenerator.Emit(OpCodes.Pop);

                _ilGenerator.Emit(OpCodes.Ldloc, _firstTimeLocal);
                _ilGenerator.Emit(OpCodes.Brfalse, afterSplitLabel);

                if (lastEntity is not null &&
                    _entityHolders.TryGetValue(_reachableEntities.HasId(entity, out _), out var entityHolder) &&
                    entityHolder.DirectAssignedRelation is not null)
                {
                    if (entityHolder.DirectAssignedRelation.ForeignKeyLocation == ForeignKeyLocation.Left &&
                        !lastEntity.HasDbGeneratedPrimaryKey)
                    {
                        _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                        _ilGenerator.Emit(OpCodes.Ldloc, lastEntityLocal);
                        _ilGenerator.Emit(OpCodes.Callvirt, lastEntity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                        WritePropertyAssigner(_ilGenerator, entityHolder.DirectAssignedRelation.ForeignKeyColumn);
                    }
                    else if (!entity.HasDbGeneratedPrimaryKey)
                    {
                        _ilGenerator.Emit(OpCodes.Ldloc, lastEntityLocal);
                        _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                        _ilGenerator.Emit(OpCodes.Callvirt, entity.GetPrimaryColumn().PropertyInfo.GetGetMethod());
                        WritePropertyAssigner(_ilGenerator, entityHolder.DirectAssignedRelation.ForeignKeyColumn);
                    }
                }

                // Add self to collection
                if (entity != _rootEntity)
                {
                    var entityCollectionField = _entityCollections[entityId];

                    _ilGenerator.Emit(OpCodes.Ldarg_0);
                    _ilGenerator.Emit(OpCodes.Ldfld, entityCollectionField);
                    _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                    _ilGenerator.Emit(OpCodes.Callvirt, entityCollectionField.FieldType.GetMethod("Add"));
                }

                for (var relationIndex = entity.Relations.Count - 1; relationIndex >= 0; relationIndex--)
                {
                    var relation = entity.Relations[relationIndex];

                    if (relation.LeftNavigationProperty is null)
                        continue;

                    entityId = _reachableEntities.HasId(relation.RightEntity, out var isNotReachable);

                    if (isNotReachable ||
                        _visitedRelations.Contains(relation.RelationId) ||
                        _vistiedEntities.Contains(entityId) ||
                        !_reachableRelations.Contains(relation.RelationId))
                        continue;

                    _visitedRelations.Add(relation.RelationId);

                    // add foreign key to collection

                    if (relation.RelationType is RelationType.ManyToOne or RelationType.OneToOne)
                    {
                        var rightEntityLocal = _ilGenerator.DeclareLocal(relation.RightEntity.EntityType);

                        _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                        _ilGenerator.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _ilGenerator.Emit(OpCodes.Stloc, rightEntityLocal);

                        WriteEntitySeperaterBase(relation.RightEntity, rightEntityLocal, entity, leftEntityLocal);
                    }
                    else
                    {
                        var startLoopBodyLabel = _ilGenerator.DefineLabel();
                        var loopConditionLabel = _ilGenerator.DefineLabel();
                        var afterNullCheckLabel = _ilGenerator.DefineLabel();

                        var iteratorLocal = _ilGenerator.DeclareLocal(typeof(int));
                        var rightEntityLocal = _ilGenerator.DeclareLocal(relation.RightEntity.EntityType);

                        // Check for entity null
                        _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                        _ilGenerator.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _ilGenerator.Emit(OpCodes.Brfalse, afterNullCheckLabel);

                        // Assign 0 to the iterator
                        _ilGenerator.Emit(OpCodes.Ldc_I4_0);
                        _ilGenerator.Emit(OpCodes.Stloc, iteratorLocal);
                        _ilGenerator.Emit(OpCodes.Br, loopConditionLabel);

                        // loop body
                        _ilGenerator.MarkLabel(startLoopBodyLabel);

                        // iterator
                        _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                        _ilGenerator.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _ilGenerator.Emit(OpCodes.Ldloc, iteratorLocal);
                        _ilGenerator.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetMethod("get_Item"));
                        _ilGenerator.Emit(OpCodes.Stloc, rightEntityLocal);

                        WriteEntitySeperaterBase(relation.RightEntity, rightEntityLocal, entity, leftEntityLocal);

                        // loop iterator increment
                        _ilGenerator.Emit(OpCodes.Ldloc, iteratorLocal);
                        _ilGenerator.Emit(OpCodes.Ldc_I4_1);
                        _ilGenerator.Emit(OpCodes.Add);
                        _ilGenerator.Emit(OpCodes.Stloc, iteratorLocal);

                        // loop condition
                        _ilGenerator.MarkLabel(loopConditionLabel);
                        _ilGenerator.Emit(OpCodes.Ldloc, iteratorLocal);
                        _ilGenerator.Emit(OpCodes.Ldloc, leftEntityLocal);
                        _ilGenerator.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _ilGenerator.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.FindProperty("Count", typeof(ICollection<>)).GetGetMethod());
                        _ilGenerator.Emit(OpCodes.Blt, startLoopBodyLabel);

                        _ilGenerator.MarkLabel(afterNullCheckLabel);
                    }
                }

                _ilGenerator.MarkLabel(afterSplitLabel);
            }

            private void WritePropertyAssigner(ILGenerator ilGenerator, EntityColumn column)
            {
                var underlyingType = Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType);

                if (underlyingType is not null)
                {
                    ilGenerator.Emit(OpCodes.Newobj, column.PropertyInfo.PropertyType.GetConstructor(new[] { underlyingType }));
                }

                if (column.Options.HasFlag(ColumnOptions.ReadOnly))
                    ilGenerator.Emit(OpCodes.Stfld, column.PropertyInfo.GetBackingField());
                else
                    ilGenerator.Emit(OpCodes.Callvirt, column.PropertyInfo.GetSetMethod(true));
            }
        }
    }
}
