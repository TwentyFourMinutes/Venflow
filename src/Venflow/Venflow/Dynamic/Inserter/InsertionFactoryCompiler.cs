using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Dynamic.IL;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionFactoryCompiler<TEntity> where TEntity : class, new()
    {
        #region ILFields

        private FieldBuilder? _npgsqlReaderTaskAwaiterField;
        private FieldBuilder NpgsqlReaderTaskAwaiterField => _npgsqlReaderTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_npgsqlAwaiter", _npgsqlReaderTaskAwaiterType, FieldAttributes.Private);

        private FieldBuilder? _boolTaskAwaiterField;
        private FieldBuilder BoolTaskAwaiterField => _boolTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_boolAwaiter", _boolTaskAwaiterType, FieldAttributes.Private);

        private FieldBuilder? _intTaskAwaiterField;
        private FieldBuilder IntTaskAwaiterField => _intTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_intAwaiter", _intTaskAwaiterType, FieldAttributes.Private);

        private FieldBuilder? _valueTaskAwaiterField;
        private FieldBuilder ValueTaskAwaiterField => _valueTaskAwaiterField ??= _stateMachineTypeBuilder.DefineField("_valueTaskAwaiter", _valueTaskAwaiterType, FieldAttributes.Private);

        private FieldBuilder? _dataReaderField;
        private FieldBuilder DataReaderField => _dataReaderField ??= _stateMachineTypeBuilder.DefineField("_dataReader", _npgsqlDataReaderType, FieldAttributes.Private);

        #endregion

        #region ILLocals

        private LocalBuilder? _dataReaderLocal;
        private LocalBuilder DataReaderLocal => _dataReaderLocal ??= _moveNextMethodIL.DeclareLocal(_npgsqlDataReaderType);

        private LocalBuilder? _npgsqlReaderTaskAwaiterLocal;
        private LocalBuilder NpgsqlReaderTaskAwaiterLocal => _npgsqlReaderTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(_npgsqlReaderTaskAwaiterType);

        private LocalBuilder? _boolTaskAwaiterLocal;
        private LocalBuilder BoolTaskAwaiterLocal => _boolTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(_boolTaskAwaiterType);

        private LocalBuilder? _intTaskAwaiterLocal;
        private LocalBuilder IntTaskAwaiterLocal => _intTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(_intTaskAwaiterType);

        private LocalBuilder? _valueTaskAwaiterLocal;
        private LocalBuilder ValueTaskAwaiterLocal => _valueTaskAwaiterLocal ??= _moveNextMethodIL.DeclareLocal(_valueTaskAwaiterType);

        private LocalBuilder? _valueTaskLocal;
        private LocalBuilder ValueTaskLocal => _valueTaskLocal ??= _moveNextMethodIL.DeclareLocal(_valueTaskType);

        private LocalBuilder? _resultTempLocal;
        private LocalBuilder ResultTempLocal => _resultTempLocal ??= _moveNextMethodIL.DeclareLocal(_intType);

        #endregion

        private InsertOptions _insertOptions;

        private readonly Entity _rootEntity;
        private readonly TypeBuilder _inserterTypeBuilder;
        private readonly TypeBuilder _stateMachineTypeBuilder;
        private readonly MethodBuilder _moveNextMethod;
        private readonly ILGenerator _moveNextMethodIL;

        private readonly Type _intType = typeof(int);
        private readonly Type _npgsqlDataReaderType = typeof(NpgsqlDataReader);

        private readonly Type _genericListType = typeof(List<>);

        private readonly Type _asyncStateMachineType = typeof(IAsyncStateMachine);
        private readonly Type _valueTaskType = typeof(ValueTask);
        private readonly Type _valueTaskAwaiterType = typeof(ValueTaskAwaiter);

        private readonly Type _asyncMethodBuilderIntType = typeof(AsyncTaskMethodBuilder<int>);

        private readonly Type _npgsqlReaderTaskAwaiterType = typeof(TaskAwaiter<NpgsqlDataReader>);
        private readonly Type _boolTaskAwaiterType = typeof(TaskAwaiter<bool>);
        private readonly Type _intTaskAwaiterType = typeof(TaskAwaiter<int>);

        private readonly ILGhostGenerator _defaultEntitySeperationGhostIL = new ILGhostGenerator();
        private readonly ILGhostGenerator _entityListAssignmentsGhostIL = new ILGhostGenerator();
        private readonly ILGhostGenerator _setListsToNullGhostIL = new ILGhostGenerator();

        private readonly Dictionary<long, FieldBuilder> _entityListDict = new Dictionary<long, FieldBuilder>();
        private readonly ObjectIDGenerator _knownEntities = new ObjectIDGenerator();
        private readonly ObjectIDGenerator _visitedSeperaterEntities = new ObjectIDGenerator();
        private readonly HashSet<uint> _visitedSeperaterRelations = new HashSet<uint>();

        internal InsertionFactoryCompiler(Entity rootEntity)
        {
            _rootEntity = rootEntity;

            _inserterTypeBuilder = TypeFactory.GetNewInserterBuilder(rootEntity.EntityName, TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);
            _stateMachineTypeBuilder = _inserterTypeBuilder.DefineNestedType("StateMachine", TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, typeof(ValueType), new[] { _asyncStateMachineType });

            _moveNextMethod = _stateMachineTypeBuilder.DefineMethod("MoveNext", MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual);
            _moveNextMethodIL = _moveNextMethod.GetILGenerator();
        }

        internal Func<NpgsqlConnection, List<TEntity>, Task<int>> CreateInserter(EntityRelationHolder[] entities, InsertOptions insertOptions)
        {
            _insertOptions = insertOptions;

            var rootEntityListType = _genericListType.MakeGenericType(_rootEntity.EntityType);

            var taskAwaiterType = typeof(TaskAwaiter<>);
            var boolTaskAwaiterType = taskAwaiterType.MakeGenericType(typeof(bool));
            var taskBoolType = typeof(Task<bool>);
            var taskIntType = typeof(Task<int>);
            var taskReaderType = typeof(Task<NpgsqlDataReader>);
            var exceptionType = typeof(Exception);

            int asyncStateIndex = 0;

            // Required StateMachine Fields
            var stateField = _stateMachineTypeBuilder.DefineField("_state", _intType, FieldAttributes.Public);
            var methodBuilderField = _stateMachineTypeBuilder.DefineField("_builder", _asyncMethodBuilderIntType, FieldAttributes.Public);

            // Custom Fields
            var rootEntityListField = _stateMachineTypeBuilder.DefineField("_" + _rootEntity.EntityName + "List", rootEntityListType, FieldAttributes.Public);

            var connectionField = _stateMachineTypeBuilder.DefineField("_connection", typeof(NpgsqlConnection), FieldAttributes.Public);
            var commandBuilderField = _stateMachineTypeBuilder.DefineField("_commandBuilder", typeof(StringBuilder), FieldAttributes.Private);
            var commandField = _stateMachineTypeBuilder.DefineField("_command", typeof(NpgsqlCommand), FieldAttributes.Private);

            var resultField = _stateMachineTypeBuilder.DefineField("insertedRows", _intType, FieldAttributes.Private);

            var stateLocal = _moveNextMethodIL.DeclareLocal(_intType);
            var exceptionLocal = _moveNextMethodIL.DeclareLocal(exceptionType);

            var endOfMethodLabel = _moveNextMethodIL.DefineLabel();
            var retOfMethodLabel = _moveNextMethodIL.DefineLabel();

            // Assign the local state from the local field => state = _state;
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, stateField);
            _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);

            _entityListDict.Add(_knownEntities.GetId(rootEntityListField, out _), rootEntityListField);

            if (HasOptionsFlag(InsertOptions.PopulateRelations))
                SeperateRootEntity(_rootEntity, rootEntityListField);

            _moveNextMethodIL.BeginExceptionBlock();

            // Switch around all the different states

            int labelCount = 0;

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i].Entity;

                if (((IPrimaryEntityColumn)entity.GetPrimaryColumn()).IsServerSideGenerated && HasOptionsFlag(InsertOptions.SetIdentityColumns))
                {
                    labelCount += 4;
                }
                else
                {
                    labelCount++;
                }
            }

            _moveNextMethodIL.Emit(OpCodes.Ldloc, stateLocal);
            var switchBuilder = _moveNextMethodIL.EmitSwitch(labelCount);

            // Default case, checking if the root entities are null or empty

            var beforeDefaultRootListCheckBodyLabel = _moveNextMethodIL.DefineLabel();
            var afterDefaultRootListCheckBodyLabel = _moveNextMethodIL.DefineLabel();

            // Assign the result 0
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
            _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);

            // Check if list is null
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, rootEntityListField);
            _moveNextMethodIL.Emit(OpCodes.Brfalse, beforeDefaultRootListCheckBodyLabel);

            // Check if list is empty

            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, rootEntityListField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, rootEntityListType.GetProperty("Count").GetGetMethod());
            _moveNextMethodIL.Emit(OpCodes.Brtrue, afterDefaultRootListCheckBodyLabel);

            _moveNextMethodIL.MarkLabel(beforeDefaultRootListCheckBodyLabel);

            // DefaultRootListCheckBody

            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            _moveNextMethodIL.MarkLabel(afterDefaultRootListCheckBodyLabel);

            // Write the List and ObjectIdGenreator Instantiations
            _entityListAssignmentsGhostIL.WriteIL(_moveNextMethodIL);

            // Write the default entity separation
            _defaultEntitySeperationGhostIL.WriteIL(_moveNextMethodIL);

            // Instantiate CommandBuilder
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Newobj, commandBuilderField.FieldType.GetConstructor(Type.EmptyTypes));
            _moveNextMethodIL.Emit(OpCodes.Stfld, commandBuilderField);

            // Instantiate Command
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Newobj, commandField.FieldType.GetConstructor(Type.EmptyTypes));
            _moveNextMethodIL.Emit(OpCodes.Stfld, commandField);

            // Assign the connection parameter to the command
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, connectionField);
            _moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Connection", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetSetMethod());

            for (int i = 0; i < entities.Length; i++)
            {
                var entityHolder = entities[i];

                var isRoot = object.ReferenceEquals(entityHolder.Entity, _rootEntity);

                var entityListField = isRoot ? rootEntityListField : _entityListDict[_knownEntities.HasId(entityHolder.Entity, out _)];

                var afterEntityInsertionLabel = _moveNextMethodIL.DefineLabel();

                // Check if the entityList is empty
                if (!isRoot)
                {
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Ble, afterEntityInsertionLabel);
                }

                if (i > 0)
                {
                    // clear the command builder
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Clear"));
                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    // clear parameters in command
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).PropertyType.GetMethod("Clear"));
                }

                var stringBuilder = new StringBuilder();

                var skipPrimaryKey = ((IPrimaryEntityColumn)entityHolder.Entity.GetPrimaryColumn()).IsServerSideGenerated && HasOptionsFlag(InsertOptions.SetIdentityColumns);

                var columnCount = entityHolder.Entity.GetColumnCount();
                var columnOffset = skipPrimaryKey ? entityHolder.Entity.GetRegularColumnOffset() : 0;

                stringBuilder.Append("INSERT INTO ")
                             .Append(entityHolder.Entity.TableName)
                             .Append(" (")
                             .Append(skipPrimaryKey ? entityHolder.Entity.NonPrimaryColumnListString : entityHolder.Entity.ColumnListString)
                             .Append(") VALUES ");

                // Outer loop to keep one single command under ushort.MaxValue parameters

                var totalLocal = _moveNextMethodIL.DeclareLocal(_intType);
                var currentLocal = _moveNextMethodIL.DeclareLocal(_intType);

                // Assign the total amount of parameters to the total local
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
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

                var iteratorElementLocal = _moveNextMethodIL.DeclareLocal(entityListField.FieldType);
                var placeholderLocal = _moveNextMethodIL.DeclareLocal(typeof(string));

                var loopConditionLabel = _moveNextMethodIL.DefineLabel();
                var startLoopBodyLabel = _moveNextMethodIL.DefineLabel();

                // loop body
                _moveNextMethodIL.MarkLabel(startLoopBodyLabel);

                // get element at iterator from list
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                _moveNextMethodIL.Emit(OpCodes.Ldloc, currentLocal);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetMethod("get_Item"));
                _moveNextMethodIL.Emit(OpCodes.Stloc, iteratorElementLocal);

                // append placeholders to command builder
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                _moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)'(');
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(char) }));
                _moveNextMethodIL.Emit(OpCodes.Pop);

                for (int k = columnOffset; k < columnCount; k++)
                {
                    var column = entityHolder.Entity.GetColumn(k);

                    // Create the placeholder
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, "@" + column.ColumnName);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, currentLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, _intType.GetMethod("ToString", Type.EmptyTypes));
                    _moveNextMethodIL.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, placeholderLocal);

                    // Write placeholder to the command builder => (@Name(n)),
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, placeholderLocal);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, columnCount == k + 1 ? "), " : ", ");
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    // Create new parameter with placeholder and add it to the parameter list
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());

                    var underlyingType = Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType);

                    if (underlyingType is { } &&
                        (underlyingType.IsEnum ||
                         underlyingType == typeof(Guid)))
                    {
                        var stringType = typeof(string);
                        var dbNullType = typeof(DBNull);

                        var propertyLocal = _moveNextMethodIL.DeclareLocal(column.PropertyInfo.PropertyType);

                        var defaultRetrieverLabel = _moveNextMethodIL.DefineLabel();
                        var afterHasValueLabel = _moveNextMethodIL.DefineLabel();

                        // Check if property has value
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, column.PropertyInfo.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Stloc, propertyLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldloca, propertyLocal);
                        _moveNextMethodIL.Emit(OpCodes.Call, propertyLocal.LocalType.GetProperty("HasValue").GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Brtrue_S, defaultRetrieverLabel);

                        // Nullable retriever
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, placeholderLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldsfld, dbNullType.GetField("Value"));
                        _moveNextMethodIL.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(dbNullType).GetConstructor(new[] { stringType, dbNullType }));
                        _moveNextMethodIL.Emit(OpCodes.Br, afterHasValueLabel);

                        // Default retriever
                        _moveNextMethodIL.MarkLabel(defaultRetrieverLabel);

                        _moveNextMethodIL.Emit(OpCodes.Ldloc, placeholderLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, column.PropertyInfo.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Stloc, propertyLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldloca, propertyLocal);
                        _moveNextMethodIL.Emit(OpCodes.Call, propertyLocal.LocalType.GetProperty("Value").GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(underlyingType).GetConstructor(new[] { stringType, underlyingType }));

                        _moveNextMethodIL.MarkLabel(afterHasValueLabel);
                    }
                    else
                    {
                        var npgsqlType = column.PropertyInfo.PropertyType.IsEnum ? Enum.GetUnderlyingType(column.PropertyInfo.PropertyType) : column.PropertyInfo.PropertyType;

                        _moveNextMethodIL.Emit(OpCodes.Ldloc, placeholderLocal);
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, column.PropertyInfo.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(npgsqlType).GetConstructor(new[] { typeof(string), npgsqlType }));
                    }

                    _moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameterCollection).GetMethod("Add", new[] { typeof(NpgsqlParameter) }));
                    _moveNextMethodIL.Emit(OpCodes.Pop);
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
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, " RETURNING \"");
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, entityHolder.Entity.GetPrimaryColumn().ColumnName);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    _moveNextMethodIL.Emit(OpCodes.Ldstr, "\";");
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
                _moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                _moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("ToString", Type.EmptyTypes));
                _moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("CommandText").GetSetMethod());

                if (skipPrimaryKey)
                {
                    // Execute Reader and use SingleRow if only one entity
                    var beforeElseLabel = _moveNextMethodIL.DefineLabel();
                    var afterElseLabel = _moveNextMethodIL.DefineLabel();

                    //TODO: Use ExecuteScalar instead of single-row
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    _moveNextMethodIL.Emit(OpCodes.Beq, beforeElseLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Br, afterElseLabel);

                    _moveNextMethodIL.MarkLabel(beforeElseLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, 8);
                    _moveNextMethodIL.MarkLabel(afterElseLabel);

                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetMethod("ExecuteReaderAsync", new[] { typeof(CommandBehavior) })); // TODO: add Cancellation Token
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, taskReaderType.GetMethod("GetAwaiter"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, NpgsqlReaderTaskAwaiterLocal);

                    var afterAwaitLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldloca, NpgsqlReaderTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, _npgsqlReaderTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);

                    // Await Handler

                    // stateField = stateLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, NpgsqlReaderTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, NpgsqlReaderTaskAwaiterField);

                    // Call AwaitUnsafeOnCompleted
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, NpgsqlReaderTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(_npgsqlReaderTaskAwaiterType, _stateMachineTypeBuilder));
                    _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, NpgsqlReaderTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, NpgsqlReaderTaskAwaiterLocal);

                    // taskAwaiterField = default
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, NpgsqlReaderTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Initobj, _npgsqlReaderTaskAwaiterType);

                    // stateField = stateLocal = -1
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    _moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, NpgsqlReaderTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, _npgsqlReaderTaskAwaiterType.GetMethod("GetResult"));

                    // store result in npgsqlDataReaderLocal
                    _moveNextMethodIL.Emit(OpCodes.Stloc, DataReaderLocal);

                    //dataReaderField = dataReaderLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, DataReaderLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, DataReaderField);

                    var iteratorField = _stateMachineTypeBuilder.DefineField("_i" + entityHolder.Entity.EntityName, _intType, FieldAttributes.Private);
                    var counterField = _stateMachineTypeBuilder.DefineField("_counter" + entityHolder.Entity.EntityName, _intType, FieldAttributes.Private);

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
                    afterAwaitLabel = _moveNextMethodIL.DefineLabel();
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, DataReaderField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _npgsqlDataReaderType.GetMethod("NextResultAsync", Type.EmptyTypes));// TODO: use CancellationToken
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, taskBoolType.GetMethod("GetAwaiter"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, boolTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);

                    // Await Handler

                    // stateField = stateLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, BoolTaskAwaiterField);

                    // Call AwaitUnsafeOnCompleted
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(boolTaskAwaiterType, _stateMachineTypeBuilder));
                    _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, BoolTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, BoolTaskAwaiterLocal);

                    // taskAwaiterField = default
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, BoolTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Initobj, boolTaskAwaiterType);

                    // stateField = stateLocal = -1
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    _moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, boolTaskAwaiterType.GetMethod("GetResult"));
                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    _moveNextMethodIL.MarkLabel(afterIfBody);

                    // read data reader
                    afterAwaitLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, DataReaderField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, DataReaderField.FieldType.GetMethod("ReadAsync", Type.EmptyTypes)); // TODO: use CancellationToken
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, taskBoolType.GetMethod("GetAwaiter"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, boolTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);

                    // Await Handler

                    // stateField = stateLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, BoolTaskAwaiterField);

                    // Call AwaitUnsafeOnCompleted
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(boolTaskAwaiterType, _stateMachineTypeBuilder));
                    _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, BoolTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, BoolTaskAwaiterLocal);

                    // taskAwaiterField = default
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, BoolTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Initobj, boolTaskAwaiterType);

                    // stateField = stateLocal = -1
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    _moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, BoolTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, boolTaskAwaiterType.GetMethod("GetResult"));
                    _moveNextMethodIL.Emit(OpCodes.Pop);

                    // get element at iterator from list
                    iteratorElementLocal = _moveNextMethodIL.DeclareLocal(entityHolder.Entity.EntityType);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetMethod("get_Item"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, iteratorElementLocal);

                    // get computed key from dataReader
                    var primaryKeyLocal = _moveNextMethodIL.DeclareLocal(entityHolder.Entity.GetPrimaryColumn().PropertyInfo.PropertyType);

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, DataReaderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _npgsqlDataReaderType.GetMethod("GetFieldValue").MakeGenericMethod(entityHolder.Entity.GetPrimaryColumn().PropertyInfo.PropertyType));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, primaryKeyLocal);

                    // Assign computed key to current element
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityHolder.Entity.GetPrimaryColumn().PropertyInfo.GetSetMethod());

                    var iteratorLocal = _moveNextMethodIL.DeclareLocal(_intType);

                    for (int k = 0; k < entityHolder.AssigningRelations.Count; k++)
                    {
                        var relation = entityHolder.AssigningRelations[k];

                        if (relation.LeftNavigationProperty is null)
                            continue;

                        // Check if navigation property is not null
                        var afterNavigationPropertyAssignmentLabel = _moveNextMethodIL.DefineLabel();
                        _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        _moveNextMethodIL.Emit(OpCodes.Brfalse, afterNavigationPropertyAssignmentLabel);

                        if (relation.RelationType == RelationType.OneToMany)
                        {
                            var nestedLoopConditionLabel = _moveNextMethodIL.DefineLabel();
                            var startNestedLoopLabel = _moveNextMethodIL.DefineLabel();

                            // Assign 0 to the nested iterator
                            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                            _moveNextMethodIL.Emit(OpCodes.Stloc, iteratorLocal);
                            _moveNextMethodIL.Emit(OpCodes.Br, nestedLoopConditionLabel);

                            // nested loop body

                            _moveNextMethodIL.MarkLabel(startNestedLoopLabel);

                            // Assign the primary key to the foreign entity
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetMethod("get_Item"));
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.ForeignKeyColumn.PropertyInfo.GetSetMethod());

                            // nested loop iterator increment
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                            _moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                            _moveNextMethodIL.Emit(OpCodes.Add);
                            _moveNextMethodIL.Emit(OpCodes.Stloc, iteratorLocal);

                            // nested loop condition
                            _moveNextMethodIL.MarkLabel(nestedLoopConditionLabel);
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetProperty("Count").GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Blt, startNestedLoopLabel);
                        }
                        else if (relation.RelationType == RelationType.OneToOne)
                        {
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                            _moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                            _moveNextMethodIL.Emit(OpCodes.Callvirt, relation.ForeignKeyColumn.PropertyInfo.GetSetMethod());
                        }

                        _moveNextMethodIL.MarkLabel(afterNavigationPropertyAssignmentLabel);
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
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

                    // Add returned rows to total inserted rows
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Add);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);

                    // dispose data reader
                    afterAwaitLabel = _moveNextMethodIL.DefineLabel();
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, DataReaderField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, _npgsqlDataReaderType.GetMethod("DisposeAsync"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, ValueTaskLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, ValueTaskLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, _valueTaskType.GetMethod("GetAwaiter"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, ValueTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, ValueTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, _valueTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);

                    // Await Handler

                    // stateField = stateLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, ValueTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, ValueTaskAwaiterField);

                    // Call AwaitUnsafeOnCompleted
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, ValueTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(_valueTaskAwaiterType, _stateMachineTypeBuilder));
                    _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, ValueTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, ValueTaskAwaiterLocal);

                    // taskAwaiterField = default
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, ValueTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Initobj, _valueTaskAwaiterType);

                    // stateField = stateLocal = -1
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    _moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, ValueTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, _valueTaskAwaiterType.GetMethod("GetResult"));
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldnull);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, DataReaderField);
                }
                else
                {
                    var afterAwaitLabel = _moveNextMethodIL.DefineLabel();

                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetMethod("ExecuteNonQueryAsync", Type.EmptyTypes)); // TODO: add Cancellation Token
                    _moveNextMethodIL.Emit(OpCodes.Callvirt, taskIntType.GetMethod("GetAwaiter"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, IntTaskAwaiterLocal);

                    _moveNextMethodIL.Emit(OpCodes.Ldloca, IntTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, _intTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    _moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);

                    // Await Handler

                    // stateField = stateLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, IntTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, IntTaskAwaiterField);

                    // Call AwaitUnsafeOnCompleted
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, IntTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(_npgsqlReaderTaskAwaiterType, _stateMachineTypeBuilder));
                    _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, IntTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, IntTaskAwaiterLocal);

                    // taskAwaterField = default
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldflda, IntTaskAwaiterField);
                    _moveNextMethodIL.Emit(OpCodes.Initobj, _intTaskAwaiterType);

                    // stateField = stateLocal = -1
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    _moveNextMethodIL.Emit(OpCodes.Dup);
                    _moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    _moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    _moveNextMethodIL.Emit(OpCodes.Ldloca, IntTaskAwaiterLocal);
                    _moveNextMethodIL.Emit(OpCodes.Call, _intTaskAwaiterType.GetMethod("GetResult"));
                    _moveNextMethodIL.Emit(OpCodes.Stloc, ResultTempLocal);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
                    _moveNextMethodIL.Emit(OpCodes.Ldloc, ResultTempLocal);
                    _moveNextMethodIL.Emit(OpCodes.Add);
                    _moveNextMethodIL.Emit(OpCodes.Stfld, resultField);
                }

                _moveNextMethodIL.MarkLabel(afterEntityInsertionLabel);
            }

            _moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            _moveNextMethodIL.BeginCatchBlock(exceptionType);

            // Store the exception in the exceptionLocal
            _moveNextMethodIL.Emit(OpCodes.Stloc, exceptionLocal);

            // Assign -2 to the state field
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4, -2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

            // Set command builder to null
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stfld, commandBuilderField);

            // Set command to null
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stfld, commandField);

            _setListsToNullGhostIL.WriteIL(_moveNextMethodIL);

            // Set the exception to the method builder
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldloc, exceptionLocal);
            _moveNextMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("SetException"));
            _moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

            _moveNextMethodIL.EndExceptionBlock();

            _moveNextMethodIL.MarkLabel(endOfMethodLabel);

            // Assign -2 to the state field
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldc_I4, -2);
            _moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

            //// Set command builder to null
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stfld, commandBuilderField);

            // Set command to null
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldnull);
            _moveNextMethodIL.Emit(OpCodes.Stfld, commandField);

            _setListsToNullGhostIL.WriteIL(_moveNextMethodIL);

            // Set the result to the method builder
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            _moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            _moveNextMethodIL.Emit(OpCodes.Ldfld, resultField);
            _moveNextMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("SetResult"));

            _moveNextMethodIL.MarkLabel(retOfMethodLabel);
            _moveNextMethodIL.Emit(OpCodes.Ret);

            #region StateMachine

            _stateMachineTypeBuilder.DefineMethodOverride(_moveNextMethod, _asyncStateMachineType.GetMethod("MoveNext"));

            var setStateMachineMethod = _stateMachineTypeBuilder.DefineMethod("SetStateMachine", MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, null, new[] { _asyncStateMachineType });
            var setStateMachineMethodIL = setStateMachineMethod.GetILGenerator();

            setStateMachineMethodIL.Emit(OpCodes.Ldarg_0);
            setStateMachineMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            setStateMachineMethodIL.Emit(OpCodes.Ldarg_1);
            setStateMachineMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("SetStateMachine"));
            setStateMachineMethodIL.Emit(OpCodes.Ret);

            _stateMachineTypeBuilder.DefineMethodOverride(setStateMachineMethod, _asyncStateMachineType.GetMethod("SetStateMachine"));

            var materializeMethod = _inserterTypeBuilder.DefineMethod("InsertAsync", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, taskIntType, new[] { typeof(NpgsqlConnection), typeof(List<TEntity>) });

            materializeMethod.SetCustomAttribute(new CustomAttributeBuilder(typeof(AsyncStateMachineAttribute).GetConstructor(new[] { typeof(Type) }), new[] { _stateMachineTypeBuilder }));

            var materializeMethodIL = materializeMethod.GetILGenerator();

            materializeMethodIL.DeclareLocal(_stateMachineTypeBuilder);

            // Create and execute the StateMachine
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static));
            materializeMethodIL.Emit(OpCodes.Stfld, methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldarg_0);
            materializeMethodIL.Emit(OpCodes.Stfld, connectionField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldarg_1);
            materializeMethodIL.Emit(OpCodes.Stfld, rootEntityListField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldc_I4_M1);
            materializeMethodIL.Emit(OpCodes.Stfld, stateField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(_stateMachineTypeBuilder));
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Call, _asyncMethodBuilderIntType.GetProperty("Task").GetGetMethod());

            materializeMethodIL.Emit(OpCodes.Ret);

            #endregion

            _stateMachineTypeBuilder.CreateType();
            var inserterType = _inserterTypeBuilder.CreateType();

            return (Func<NpgsqlConnection, List<TEntity>, Task<int>>)inserterType.GetMethod("InsertAsync").CreateDelegate(typeof(Func<NpgsqlConnection, List<TEntity>, Task<int>>));
        }

        private void SeperateRootEntity(Entity entity, FieldBuilder? entityListField = null, LocalBuilder? entityListLocal = null)
        {
            if (entity.Relations is null)
                return;

            _ = _visitedSeperaterEntities.GetId(entity, out var firstTime);

            if (!firstTime)
                return;


            var hasValidRelation = false;

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                if (relation.LeftNavigationProperty is null)
                    continue;

                if (!_visitedSeperaterRelations.Contains(relation.RelationId))
                {
                    hasValidRelation = true;

                    _visitedSeperaterRelations.Add(relation.RelationId);
                }
            }

            if (!HasOptionsFlag(InsertOptions.PopulateRelations) && !hasValidRelation)
            {
                return;
            }

            var iteratorLocal = _moveNextMethodIL.DeclareLocal(_intType);
            var iteratorElementLocal = _moveNextMethodIL.DeclareLocal(entity.EntityType);

            var loopConditionLabel = _moveNextMethodIL.DefineLabel();
            var startLoopBodyLabel = _moveNextMethodIL.DefineLabel();

            // Assign 0 to the iterator
            _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldc_I4_0);
            _defaultEntitySeperationGhostIL.Emit(OpCodes.Stloc, iteratorLocal);
            _defaultEntitySeperationGhostIL.Emit(OpCodes.Br, loopConditionLabel);

            // loop body
            _defaultEntitySeperationGhostIL.MarkLabel(startLoopBodyLabel);

            if (entityListField is { })
            {
                // get element at iterator from list
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, entityListField);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorLocal);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetMethod("get_Item"));
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Stloc, iteratorElementLocal);
            }
            else if (entityListLocal is { })
            {
                // get element at iterator from list
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityListLocal);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorLocal);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, entityListLocal.LocalType.GetMethod("get_Item"));
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Stloc, iteratorElementLocal);
            }

            SeperateEntities(entity, iteratorElementLocal);

            // loop iterator increment
            _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorLocal);
            _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldc_I4_1);
            _defaultEntitySeperationGhostIL.Emit(OpCodes.Add);
            _defaultEntitySeperationGhostIL.Emit(OpCodes.Stloc, iteratorLocal);

            // loop condition
            _defaultEntitySeperationGhostIL.MarkLabel(loopConditionLabel);
            _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorLocal);

            if (entityListField is { })
            {
                // get element at iterator from list
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, entityListField);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Blt, startLoopBodyLabel);
            }
            else if (entityListLocal is { })
            {
                // get element at iterator from list
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityListLocal);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, entityListLocal.LocalType.GetProperty("Count").GetGetMethod());
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Blt, startLoopBodyLabel);
            }
        }

        private void SeperateEntities(Entity entity, LocalBuilder rootLocal)
        {
            if (entity.Relations is null)
                return;

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                FieldBuilder? newEntityListField;

                if (relation.LeftNavigationProperty is null)
                {
                    continue;
                }

                var id = _knownEntities.GetId(relation.RightEntity, out var isNew);

                var entityLocal = _moveNextMethodIL.DeclareLocal(relation.LeftNavigationProperty.PropertyType);

                // Store navigation property in local
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, rootLocal);
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                _defaultEntitySeperationGhostIL.Emit(OpCodes.Stloc, entityLocal);

                if (relation.RelationType == RelationType.OneToMany)
                {
                    if (isNew)
                    {
                        newEntityListField = _stateMachineTypeBuilder.DefineField("_" + entity.EntityName + relation.RightEntity.EntityName + "List", relation.LeftNavigationProperty.PropertyType, FieldAttributes.Private);

                        _entityListDict.Add(id, newEntityListField);
                    }
                    else
                    {
                        newEntityListField = _entityListDict[id];
                    }

                    var afterListBodyLabel = _moveNextMethodIL.DefineLabel();

                    // Check if list is null
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Brfalse, afterListBodyLabel);

                    // Check if list is empty
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetProperty("Count").GetGetMethod());
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldc_I4_0);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ble, afterListBodyLabel);

                    // Assign the entities to the local list
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, newEntityListField);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, newEntityListField.FieldType.GetMethod("AddRange"));

                    SeperateRootEntity(relation.RightEntity, null, entityLocal);

                    _defaultEntitySeperationGhostIL.MarkLabel(afterListBodyLabel);
                }
                else if (relation.RelationType == RelationType.ManyToOne)
                {
                    if (isNew)
                    {
                        newEntityListField = _stateMachineTypeBuilder.DefineField("_" + entity.EntityName + relation.RightEntity.EntityName + "List", _genericListType.MakeGenericType(relation.LeftNavigationProperty.PropertyType), FieldAttributes.Private);

                        _entityListDict.Add(id, newEntityListField);
                    }
                    else
                    {
                        newEntityListField = _entityListDict[id];
                    }

                    var visitedEntitiesLocal = _moveNextMethodIL.DeclareLocal(typeof(ObjectIDGenerator));

                    // Instantiate visitedEntitiesLocal
                    _entityListAssignmentsGhostIL.Emit(OpCodes.Newobj, typeof(ObjectIDGenerator).GetConstructor(Type.EmptyTypes));
                    _entityListAssignmentsGhostIL.Emit(OpCodes.Stloc, visitedEntitiesLocal);

                    var isVisitedEntityLocal = _moveNextMethodIL.DeclareLocal(typeof(bool));

                    var afterCheckBodyLabel = _moveNextMethodIL.DefineLabel();

                    // Check if navigation property is set
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Brfalse, afterCheckBodyLabel);

                    // Check if instance was already visited
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, visitedEntitiesLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloca, isVisitedEntityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, typeof(ObjectIDGenerator).GetMethod("GetId"));
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Pop);

                    var afterNotVisistedLabel = _moveNextMethodIL.DefineLabel();

                    // Check if instance hasn't been visited yet
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, isVisitedEntityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Brfalse, afterNotVisistedLabel);

                    if (HasOptionsFlag(InsertOptions.PopulateRelations) && relation.RightNavigationProperty is { })
                    {
                        // Assign root to entity navigation
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Newobj, relation.RightNavigationProperty.PropertyType.GetConstructor(Type.EmptyTypes));
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetSetMethod());
                    }

                    // Assign entity to the list
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, newEntityListField);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, newEntityListField.FieldType.GetMethod("Add"));

                    if (relation.RightEntity.Relations is { })
                    {
                        _ = _visitedSeperaterEntities.GetId(relation.RightEntity, out var firstTime);

                        if (firstTime)
                        {
                            var hasValidRelation = false;

                            for (int k = 0; k < relation.RightEntity.Relations.Count; k++)
                            {
                                var foreignRelation = relation.RightEntity.Relations[k];

                                if (foreignRelation.LeftNavigationProperty is null)
                                    continue;

                                if (!_visitedSeperaterRelations.Contains(foreignRelation.RelationId))
                                {
                                    hasValidRelation = true;
                                }

                                _visitedSeperaterRelations.Add(foreignRelation.RelationId);
                            }

                            if (hasValidRelation)
                            {
                                SeperateEntities(relation.RightEntity, entityLocal);
                            }
                        }
                    }

                    _defaultEntitySeperationGhostIL.MarkLabel(afterNotVisistedLabel);

                    if (HasOptionsFlag(InsertOptions.PopulateRelations) && relation.RightNavigationProperty is { })
                    {
                        // Assign root to entity navigation
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, rootLocal);
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.PropertyType.GetMethod("Add"));
                    }

                    _defaultEntitySeperationGhostIL.MarkLabel(afterCheckBodyLabel);
                }
                else
                {
                    if (isNew)
                    {
                        newEntityListField = _stateMachineTypeBuilder.DefineField("_" + entity.EntityName + relation.RightEntity.EntityName + "List", _genericListType.MakeGenericType(relation.LeftNavigationProperty.PropertyType), FieldAttributes.Private);

                        _entityListDict.Add(id, newEntityListField);
                    }
                    else
                    {
                        newEntityListField = _entityListDict[id];
                    }

                    var afterCheckBodyLabel = _moveNextMethodIL.DefineLabel();

                    // Check if navigation property is set
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Brfalse, afterCheckBodyLabel);

                    // Assign entity to the list
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, newEntityListField);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                    _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, newEntityListField.FieldType.GetMethod("Add"));

                    if (relation.RightEntity.Relations is { })
                    {
                        _ = _visitedSeperaterEntities.GetId(relation.RightEntity, out var firstTime);

                        if (firstTime)
                        {
                            var hasValidRelation = false;

                            for (int k = 0; k < relation.RightEntity.Relations.Count; k++)
                            {
                                var foreignRelation = relation.RightEntity.Relations[k];

                                if (foreignRelation.LeftNavigationProperty is null)
                                    continue;

                                if (!_visitedSeperaterRelations.Contains(foreignRelation.RelationId))
                                {
                                    hasValidRelation = true;
                                }

                                _visitedSeperaterRelations.Add(foreignRelation.RelationId);
                            }

                            if (hasValidRelation)
                            {
                                SeperateEntities(relation.RightEntity, entityLocal);
                            }
                        }
                    }

                    if (HasOptionsFlag(InsertOptions.PopulateRelations) && relation.RightNavigationProperty is { })
                    {
                        // Assign root to entity navigation
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, rootLocal);
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.PropertyType.GetMethod("Add"));
                    }

                    if (HasOptionsFlag(InsertOptions.PopulateRelations) && relation.RightNavigationProperty is { })
                    {
                        // Assign root to entity navigation
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, entityLocal);
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Newobj, relation.RightNavigationProperty.PropertyType.GetConstructor(Type.EmptyTypes));
                        _defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetSetMethod());
                    }

                    _defaultEntitySeperationGhostIL.MarkLabel(afterCheckBodyLabel);
                }

                // Instantiate entityListField
                _entityListAssignmentsGhostIL.Emit(OpCodes.Ldarg_0);
                _entityListAssignmentsGhostIL.Emit(OpCodes.Newobj, newEntityListField.FieldType.GetConstructor(Type.EmptyTypes));
                _entityListAssignmentsGhostIL.Emit(OpCodes.Stfld, newEntityListField);

                // Set the entity list to null
                _setListsToNullGhostIL.Emit(OpCodes.Ldarg_0);
                _setListsToNullGhostIL.Emit(OpCodes.Ldnull);
                _setListsToNullGhostIL.Emit(OpCodes.Stfld, newEntityListField);
            }
        }

        private bool HasOptionsFlag(InsertOptions flag)
        {
            return (_insertOptions & flag) == flag;
        }
    }
}
