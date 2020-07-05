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
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Net.Http.Headers;
using System.Data;

namespace Venflow.Dynamic
{
    internal class InsertionFactory<TEntity> where TEntity : class
    {
        private Func<NpgsqlConnection, List<TEntity>, Task<int>>? _inserter;

        private readonly Entity<TEntity> _entity;

        internal InsertionFactory(Entity<TEntity> entity)
        {
            _entity = entity;
        }

        internal Func<NpgsqlConnection, List<TEntity>, Task<int>> GetOrCreateInserter(DbConfiguration dbConfiguration)
        {
            if (_entity.Relations is { })
            {
                if (_inserter is null)
                {
                    var sourceCompiler = new InsertionSourceCompiler();

                    sourceCompiler.Compile(_entity);

                    return _inserter = CreateInserter(_entity, sourceCompiler.GenerateSortedEntities());
                }
                else
                {
                    return _inserter;
                }
            }
            else
            {
                //Create single inserter
                throw new NotImplementedException();
            }
        }

        private Func<NpgsqlConnection, List<TEntity>, Task<int>> CreateInserter(Entity rootEntity, EntityRelationHolder[] entities)
        {
            var genericListType = typeof(List<>);
            var rootEntityListType = genericListType.MakeGenericType(rootEntity.EntityType);

            var intType = typeof(int);
            var asyncStateMachineType = typeof(IAsyncStateMachine);
            var asyncMethodBuilderIntType = typeof(AsyncTaskMethodBuilder<>).MakeGenericType(intType);
            var npgsqlDataReaderType = typeof(NpgsqlDataReader);
            var taskAwaiterType = typeof(TaskAwaiter<>);
            var npgsqlTaskAwaiterType = taskAwaiterType.MakeGenericType(npgsqlDataReaderType);
            var boolTaskAwaiterType = taskAwaiterType.MakeGenericType(typeof(bool));
            var intTaskAwaiterType = taskAwaiterType.MakeGenericType(intType);
            var valueTaskAwaiterType = typeof(ValueTaskAwaiter);
            var valueTaskType = typeof(ValueTask);
            var taskBoolType = typeof(Task<bool>);
            var taskIntType = typeof(Task<int>);
            var taskReaderType = typeof(Task<NpgsqlDataReader>);
            var exceptionType = typeof(Exception);

            int asyncStateIndex = 0;

            var inserterTypeBuilder = TypeFactory.GetNewInserterBuilder(_entity.EntityName,
                TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Abstract |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            var stateMachineTypeBuilder = inserterTypeBuilder.DefineNestedType("StateMachine",
                TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, typeof(ValueType),
                new[] { asyncStateMachineType });

            // Required StateMachine Fields
            var stateField = stateMachineTypeBuilder.DefineField("_state", intType, FieldAttributes.Public);
            var methodBuilderField = stateMachineTypeBuilder.DefineField("_builder", asyncMethodBuilderIntType, FieldAttributes.Public);

            FieldBuilder? npgsqlTaskAwaiterField = default;

            FieldBuilder GetNpgsqlTaskAwaiterField()
            {
                if (npgsqlTaskAwaiterField is null)
                    npgsqlTaskAwaiterField = stateMachineTypeBuilder.DefineField("_npgsqlAwaiter", npgsqlTaskAwaiterType, FieldAttributes.Private);

                return npgsqlTaskAwaiterField;
            }

            FieldBuilder? boolTaskAwaiterField = default;

            FieldBuilder GetBoolTaskAwaiterField()
            {
                if (boolTaskAwaiterField is null)
                    boolTaskAwaiterField = stateMachineTypeBuilder.DefineField("_boolAwaiter", boolTaskAwaiterType, FieldAttributes.Private);

                return boolTaskAwaiterField;
            }

            FieldBuilder? intTaskAwaiterField = default;

            FieldBuilder GetIntTaskAwaiterField()
            {
                if (intTaskAwaiterField is null)
                    intTaskAwaiterField = stateMachineTypeBuilder.DefineField("_intAwaiter", intTaskAwaiterType, FieldAttributes.Private);

                return intTaskAwaiterField;
            }

            FieldBuilder? valueTaskAwaiterField = default;

            FieldBuilder GetValueTaskAwaiterField()
            {
                if (valueTaskAwaiterField is null)
                    valueTaskAwaiterField = stateMachineTypeBuilder.DefineField("_valueTaskAwaiter", valueTaskAwaiterType, FieldAttributes.Private);

                return valueTaskAwaiterField;
            }

            // Custom Fields
            var rootEntityListField = stateMachineTypeBuilder.DefineField("_" + rootEntity.EntityName + "List", rootEntityListType, FieldAttributes.Public);

            var connectionField = stateMachineTypeBuilder.DefineField("_connection", typeof(NpgsqlConnection), FieldAttributes.Public);
            var commandBuilderField = stateMachineTypeBuilder.DefineField("_commandBuilder", typeof(StringBuilder), FieldAttributes.Private);
            var commandField = stateMachineTypeBuilder.DefineField("_command", typeof(NpgsqlCommand), FieldAttributes.Private);
            FieldBuilder? dataReaderField = default;

            FieldBuilder GetDataReaderField()
            {
                if (dataReaderField is null)
                    dataReaderField = stateMachineTypeBuilder.DefineField("_dataReader", npgsqlDataReaderType, FieldAttributes.Private);

                return dataReaderField;
            }

            var moveNextMethod = stateMachineTypeBuilder.DefineMethod("MoveNext",
                MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual);
            var moveNextMethodIL = moveNextMethod.GetILGenerator();

            var stateLocal = moveNextMethodIL.DeclareLocal(intType);
            var resultLocal = moveNextMethodIL.DeclareLocal(intType);
            var exceptionLocal = moveNextMethodIL.DeclareLocal(exceptionType);
            LocalBuilder? dataReaderLocal = default;

            LocalBuilder GetDataReaderLocal()
            {
                if (dataReaderLocal is null)
                    dataReaderLocal = moveNextMethodIL.DeclareLocal(npgsqlDataReaderType);

                return dataReaderLocal;
            }

            LocalBuilder? npgsqlTaskAwaiterLocal = default;

            LocalBuilder GetNpgsqlTaskAwaiterLocal()
            {
                if (npgsqlTaskAwaiterLocal is null)
                    npgsqlTaskAwaiterLocal = moveNextMethodIL.DeclareLocal(npgsqlTaskAwaiterType);

                return npgsqlTaskAwaiterLocal;
            }

            LocalBuilder? boolTaskAwaiterLocal = default;

            LocalBuilder GetBoolTaskAwaiterLocal()
            {
                if (boolTaskAwaiterLocal is null)
                    boolTaskAwaiterLocal = moveNextMethodIL.DeclareLocal(boolTaskAwaiterType);

                return boolTaskAwaiterLocal;
            }

            LocalBuilder? intTaskAwaiterLocal = default;

            LocalBuilder GetIntTaskAwaiterLocal()
            {
                if (intTaskAwaiterLocal is null)
                    intTaskAwaiterLocal = moveNextMethodIL.DeclareLocal(intTaskAwaiterType);

                return intTaskAwaiterLocal;
            }

            LocalBuilder? valueTaskAwaiterLocal = default;

            LocalBuilder GetValueTaskAwaiterLocal()
            {
                if (valueTaskAwaiterLocal is null)
                    valueTaskAwaiterLocal = moveNextMethodIL.DeclareLocal(valueTaskAwaiterType);

                return valueTaskAwaiterLocal;
            }

            LocalBuilder? valueTaskLocal = default;

            LocalBuilder GetValueTaskLocal()
            {
                if (valueTaskLocal is null)
                    valueTaskLocal = moveNextMethodIL.DeclareLocal(valueTaskType);

                return valueTaskLocal;
            }

            var endOfMethodLabel = moveNextMethodIL.DefineLabel();
            var retOfMethodLabel = moveNextMethodIL.DefineLabel();

            // Assign the local state from the local field => state = _state;
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, stateField);
            moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);

            var defaultEntitySeperationGhostIL = new ILGhostGenerator();
            var entityListAssignmentsGhostIL = new ILGhostGenerator();

            var entityListDict = new Dictionary<long, FieldBuilder>();
            var knownEntities = new ObjectIDGenerator();

            var setListsToNullGhostIL = new ILGhostGenerator();

            entityListDict.Add(knownEntities.GetId(rootEntityListField, out _), rootEntityListField);

            SeperateEntities(rootEntity, true);

            void SeperateEntities(Entity entity, bool isRoot)
            {
                var iteratorLocal = moveNextMethodIL.DeclareLocal(intType);
                var iteratorElementLocal = moveNextMethodIL.DeclareLocal(entity.EntityType);

                var loopConditionLabel = moveNextMethodIL.DefineLabel();
                var startLoopBodyLabel = moveNextMethodIL.DefineLabel();

                var listField = isRoot ? rootEntityListField : null;

                // Assign 0 to the iterator
                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldc_I4_0);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Stloc, iteratorLocal);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Br, loopConditionLabel);

                // loop body
                defaultEntitySeperationGhostIL.MarkLabel(startLoopBodyLabel);

                // get element at iterator from list
                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, listField);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorLocal);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, listField.FieldType.GetMethod("get_Item"));
                defaultEntitySeperationGhostIL.Emit(OpCodes.Stloc, iteratorElementLocal);

                for (int i = 0; i < entity.Relations.Count; i++)
                {
                    var relation = entity.Relations[i];

                    if (relation.LeftNavigationProperty is null)
                        continue;

                    FieldBuilder? entityListField;

                    if (relation.RelationType == RelationType.OneToMany)
                    {
                        entityListField = stateMachineTypeBuilder.DefineField("_" + entity.EntityName + relation.RightEntity.EntityName + "List", relation.LeftNavigationProperty.PropertyType, FieldAttributes.Private);

                        var afterListBodyLabel = moveNextMethodIL.DefineLabel();

                        // Check if list is null
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Brfalse, afterListBodyLabel);

                        // Check if list is empty
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetProperty("Count").GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldc_I4_0);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ble, afterListBodyLabel);

                        //Assign the entities to the local list
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, entityListField);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetMethod("AddRange"));

                        defaultEntitySeperationGhostIL.MarkLabel(afterListBodyLabel);
                    }
                    else
                    {
                        entityListField = stateMachineTypeBuilder.DefineField("_" + entity.EntityName + relation.RightEntity.EntityName + "List", genericListType.MakeGenericType(relation.LeftNavigationProperty.PropertyType), FieldAttributes.Private);

                        var visitedEntitiesLocal = moveNextMethodIL.DeclareLocal(typeof(ObjectIDGenerator));

                        // Instantiate visitedEntitiesLocal
                        entityListAssignmentsGhostIL.Emit(OpCodes.Newobj, typeof(ObjectIDGenerator).GetConstructor(Type.EmptyTypes));
                        entityListAssignmentsGhostIL.Emit(OpCodes.Stloc, visitedEntitiesLocal);

                        var isVisitedEntityLocal = moveNextMethodIL.DeclareLocal(typeof(bool));

                        var afterCheckBodyLabel = moveNextMethodIL.DefineLabel();

                        // Check if navigation property is set
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Brfalse, afterCheckBodyLabel);

                        // Check if instance was already visited
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, visitedEntitiesLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloca, isVisitedEntityLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, typeof(ObjectIDGenerator).GetMethod("GetId"));
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Pop);

                        // Check if instance hasn't been visited yet
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, isVisitedEntityLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Brfalse, afterCheckBodyLabel);

                        // Assign entity to the list
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, entityListField);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetMethod("Add"));

                        defaultEntitySeperationGhostIL.MarkLabel(afterCheckBodyLabel);
                    }

                    var id = knownEntities.GetId(relation.RightEntity, out var isNew);

                    if (isNew)
                    {
                        entityListDict.Add(id, entityListField);
                    }

                    // Instantiate entityListField
                    entityListAssignmentsGhostIL.Emit(OpCodes.Ldarg_0);
                    entityListAssignmentsGhostIL.Emit(OpCodes.Newobj, entityListField.FieldType.GetConstructor(Type.EmptyTypes));
                    entityListAssignmentsGhostIL.Emit(OpCodes.Stfld, entityListField);

                    // Set the entity list to null
                    setListsToNullGhostIL.Emit(OpCodes.Ldarg_0);
                    setListsToNullGhostIL.Emit(OpCodes.Ldnull);
                    setListsToNullGhostIL.Emit(OpCodes.Stfld, entityListField);

                    // TODO: call the recursive method
                }

                // loop iterator increment

                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorLocal);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldc_I4_1);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Add);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Stloc, iteratorLocal);

                // loop condition
                defaultEntitySeperationGhostIL.MarkLabel(loopConditionLabel);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorLocal);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);

                defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, listField);
                defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, listField.FieldType.GetProperty("Count").GetGetMethod());
                defaultEntitySeperationGhostIL.Emit(OpCodes.Blt, startLoopBodyLabel);
            }
            void SeperateEntity(Entity entity)
            {
                // TODO
            }

            moveNextMethodIL.BeginExceptionBlock();

            // Switch around all the different states

            int labelCount = 0;

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i].Entity;

                if (((IPrimaryEntityColumn)entity.GetPrimaryColumn()).IsServerSideGenerated)
                {
                    labelCount += 3;
                }
                else
                {
                    labelCount++;
                }
            }

            moveNextMethodIL.Emit(OpCodes.Ldloc, stateLocal);
            var switchBuilder = moveNextMethodIL.EmitSwitch(labelCount);

            // Default case, checking if the root entities are null or empty

            var beforeDefaultRootListCheckBodyLabel = moveNextMethodIL.DefineLabel();
            var afterDefaultRootListCheckBodyLabel = moveNextMethodIL.DefineLabel();

            // Check if list is null
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, rootEntityListField);
            moveNextMethodIL.Emit(OpCodes.Brfalse, beforeDefaultRootListCheckBodyLabel);

            // Check if list is empty

            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, rootEntityListField);
            moveNextMethodIL.Emit(OpCodes.Callvirt, rootEntityListType.GetProperty("Count").GetGetMethod());
            moveNextMethodIL.Emit(OpCodes.Brtrue, afterDefaultRootListCheckBodyLabel);

            moveNextMethodIL.MarkLabel(beforeDefaultRootListCheckBodyLabel);

            // DefaultRootListCheckBody

            moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
            moveNextMethodIL.Emit(OpCodes.Stloc, resultLocal);
            moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            moveNextMethodIL.MarkLabel(afterDefaultRootListCheckBodyLabel);

            // Write the List and ObjectIdGenreator Instantiations
            entityListAssignmentsGhostIL.WriteIL(moveNextMethodIL);
            // Write the default entity separation
            defaultEntitySeperationGhostIL.WriteIL(moveNextMethodIL);

            // Instantiate CommandBuilder
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Newobj, commandBuilderField.FieldType.GetConstructor(Type.EmptyTypes));
            moveNextMethodIL.Emit(OpCodes.Stfld, commandBuilderField);

            // Instantiate Command
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Newobj, commandField.FieldType.GetConstructor(Type.EmptyTypes));
            moveNextMethodIL.Emit(OpCodes.Stfld, commandField);

            // Assign the connection parameter to the command
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, connectionField);
            moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Connection", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetSetMethod());

            for (int i = 0; i < entities.Length; i++)
            {
                var entityHolder = entities[i];

                var isRoot = object.ReferenceEquals(entityHolder.Entity, rootEntity);

                var entityListField = isRoot ? rootEntityListField : entityListDict[knownEntities.HasId(entityHolder.Entity, out _)];

                var afterEntityInsertionLabel = moveNextMethodIL.DefineLabel();

                // Check if the entityList is empty
                if (!isRoot)
                {
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    moveNextMethodIL.Emit(OpCodes.Ble, afterEntityInsertionLabel);
                }

                if (i > 0)
                {
                    // clear the command builder
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Clear"));
                    moveNextMethodIL.Emit(OpCodes.Pop);

                    // clear parameters in command
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).PropertyType.GetMethod("Clear"));
                }

                var stringBuilder = new StringBuilder();

                var skipPrimaryKey = ((IPrimaryEntityColumn)entityHolder.Entity.GetPrimaryColumn()).IsServerSideGenerated;

                stringBuilder.Append("INSERT INTO ")
                             .Append(entityHolder.Entity.TableName)
                             .Append(" (")
                             .Append(skipPrimaryKey ? entityHolder.Entity.NonPrimaryColumnListString : entityHolder.Entity.ColumnListString)
                             .Append(") VALUES ");

                // Append base Insert Command to command builder
                moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                moveNextMethodIL.Emit(OpCodes.Ldstr, stringBuilder.ToString());
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                moveNextMethodIL.Emit(OpCodes.Pop);

                var iteratorLocal = moveNextMethodIL.DeclareLocal(intType);
                var iteratorElementLocal = moveNextMethodIL.DeclareLocal(entityListField.FieldType);
                var placeholderLocal = moveNextMethodIL.DeclareLocal(typeof(string));

                var loopConditionLabel = moveNextMethodIL.DefineLabel();
                var startLoopBodyLabel = moveNextMethodIL.DefineLabel();

                // Assign 0 to the iterator
                moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                moveNextMethodIL.Emit(OpCodes.Stloc, iteratorLocal);
                moveNextMethodIL.Emit(OpCodes.Br, loopConditionLabel);

                // loop body
                moveNextMethodIL.MarkLabel(startLoopBodyLabel);

                // get element at iterator from list
                moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetMethod("get_Item"));
                moveNextMethodIL.Emit(OpCodes.Stloc, iteratorElementLocal);

                // append placeholders to command builder
                moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)'(');
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(char) }));
                moveNextMethodIL.Emit(OpCodes.Pop);

                var columnCount = entityHolder.Entity.GetColumnCount();

                for (int k = skipPrimaryKey ? entityHolder.Entity.GetRegularColumnOffset() : 0; k < columnCount; k++)
                {
                    var column = entityHolder.Entity.GetColumn(k);

                    // Create the placeholder
                    moveNextMethodIL.Emit(OpCodes.Ldstr, "@" + column.ColumnName);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, iteratorLocal);
                    moveNextMethodIL.Emit(OpCodes.Call, intType.GetMethod("ToString", Type.EmptyTypes));
                    moveNextMethodIL.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Stloc, placeholderLocal);

                    // Write placeholder to the command builder => (@Name(n)),
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, placeholderLocal);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Ldstr, columnCount == k + 1 ? "), " : ", ");
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Pop);

                    // Create new parameter with placeholder and add it to the parameter list
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Parameters", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Ldloc, placeholderLocal);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, column.PropertyInfo.GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(column.PropertyInfo.PropertyType).GetConstructor(new[] { typeof(string), column.PropertyInfo.PropertyType }));
                    moveNextMethodIL.Emit(OpCodes.Callvirt, typeof(NpgsqlParameterCollection).GetMethod("Add", new[] { typeof(NpgsqlParameter) }));
                    moveNextMethodIL.Emit(OpCodes.Pop);
                }

                // loop iterator increment

                moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                moveNextMethodIL.Emit(OpCodes.Add);
                moveNextMethodIL.Emit(OpCodes.Stloc, iteratorLocal);

                // loop condition
                moveNextMethodIL.MarkLabel(loopConditionLabel);
                moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                moveNextMethodIL.Emit(OpCodes.Ldarg_0);

                moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

                // Remove the last the values form the command string e.g. ", "
                moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                moveNextMethodIL.Emit(OpCodes.Dup);
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetProperty("Length").GetGetMethod());
                moveNextMethodIL.Emit(OpCodes.Ldc_I4_2);
                moveNextMethodIL.Emit(OpCodes.Sub);
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetProperty("Length").GetSetMethod());

                if (skipPrimaryKey)
                {
                    // Append " RETURNING \"PrimaryKey\""
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    moveNextMethodIL.Emit(OpCodes.Ldstr, " RETURNING \"");
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Ldstr, entityHolder.Entity.GetPrimaryColumn().ColumnName);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Ldstr, "\";");
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Pop);
                }

                // Assign the commandBuilder text to the command
                moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("ToString", Type.EmptyTypes));
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("CommandText").GetSetMethod());

                if (skipPrimaryKey)
                {
                    // Execute Reader and use SingleRow if only one entity
                    var beforeElseLabel = moveNextMethodIL.DefineLabel();
                    var afterElseLabel = moveNextMethodIL.DefineLabel();

                    //TODO: Use ExecuteScalar instead of single-row
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    moveNextMethodIL.Emit(OpCodes.Beq, beforeElseLabel);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    moveNextMethodIL.Emit(OpCodes.Br, afterElseLabel);

                    moveNextMethodIL.MarkLabel(beforeElseLabel);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4, 8);
                    moveNextMethodIL.MarkLabel(afterElseLabel);

                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetMethod("ExecuteReaderAsync", new[] { typeof(CommandBehavior) })); // TODO: add Cancellation Token
                    moveNextMethodIL.Emit(OpCodes.Callvirt, taskReaderType.GetMethod("GetAwaiter"));
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetNpgsqlTaskAwaiterLocal());

                    var afterAwaitLabel = moveNextMethodIL.DefineLabel();

                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetNpgsqlTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, npgsqlTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);

                    // Await Handler

                    // stateField = stateLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    moveNextMethodIL.Emit(OpCodes.Dup);
                    moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, GetNpgsqlTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Stfld, GetNpgsqlTaskAwaiterField());

                    // Call AwaitUnsafeOnCompleted
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetNpgsqlTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Call, asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(npgsqlTaskAwaiterType, stateMachineTypeBuilder));
                    moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, GetNpgsqlTaskAwaiterField());
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetNpgsqlTaskAwaiterLocal());

                    // taskAwaiterField = default
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldflda, GetNpgsqlTaskAwaiterField());
                    moveNextMethodIL.Emit(OpCodes.Initobj, npgsqlTaskAwaiterType);

                    // stateField = stateLocal = -1
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    moveNextMethodIL.Emit(OpCodes.Dup);
                    moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetNpgsqlTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, npgsqlTaskAwaiterType.GetMethod("GetResult"));

                    // store result in npgsqlDataReaderLocal
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetDataReaderLocal());

                    //dataReaderField = dataReaderLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, GetDataReaderLocal());
                    moveNextMethodIL.Emit(OpCodes.Stfld, GetDataReaderField());

                    var iteratorField = stateMachineTypeBuilder.DefineField("_i" + entityHolder.Entity.EntityName, intType, FieldAttributes.Private);

                    loopConditionLabel = moveNextMethodIL.DefineLabel();
                    startLoopBodyLabel = moveNextMethodIL.DefineLabel();

                    // Assign 0 to the iterator
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    moveNextMethodIL.Emit(OpCodes.Stfld, iteratorField);
                    moveNextMethodIL.Emit(OpCodes.Br, loopConditionLabel);

                    // loop body
                    moveNextMethodIL.MarkLabel(startLoopBodyLabel);

                    // read data reader
                    afterAwaitLabel = moveNextMethodIL.DefineLabel();

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, dataReaderField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, dataReaderField.FieldType.GetMethod("ReadAsync", Type.EmptyTypes)); // TODO: use CancellationToken
                    moveNextMethodIL.Emit(OpCodes.Callvirt, taskBoolType.GetMethod("GetAwaiter"));
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetBoolTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetBoolTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, boolTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);

                    // Await Handler

                    // stateField = stateLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    moveNextMethodIL.Emit(OpCodes.Dup);
                    moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, GetBoolTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Stfld, GetBoolTaskAwaiterField());

                    // Call AwaitUnsafeOnCompleted
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetBoolTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Call, asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(boolTaskAwaiterType, stateMachineTypeBuilder));
                    moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, GetBoolTaskAwaiterField());
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetBoolTaskAwaiterLocal());

                    // taskAwaiterField = default
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldflda, GetBoolTaskAwaiterField());
                    moveNextMethodIL.Emit(OpCodes.Initobj, boolTaskAwaiterType);

                    // stateField = stateLocal = -1
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    moveNextMethodIL.Emit(OpCodes.Dup);
                    moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetBoolTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, boolTaskAwaiterType.GetMethod("GetResult"));
                    moveNextMethodIL.Emit(OpCodes.Pop);

                    // get element at iterator from list
                    iteratorElementLocal = moveNextMethodIL.DeclareLocal(entityHolder.Entity.EntityType);

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetMethod("get_Item"));
                    moveNextMethodIL.Emit(OpCodes.Stloc, iteratorElementLocal);

                    // get computed key from dataReader
                    var primaryKeyLocal = moveNextMethodIL.DeclareLocal(entityHolder.Entity.GetPrimaryColumn().PropertyInfo.PropertyType);

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, GetDataReaderField());
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, npgsqlDataReaderType.GetMethod("GetFieldValue").MakeGenericMethod(entityHolder.Entity.GetPrimaryColumn().PropertyInfo.PropertyType));
                    moveNextMethodIL.Emit(OpCodes.Stloc, primaryKeyLocal);

                    // Assign computed key to current element
                    moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, entityHolder.Entity.GetPrimaryColumn().PropertyInfo.GetSetMethod());

                    // TODO: Cover from null right entities

                    for (int k = 0; k < entityHolder.AssigningRelations.Count; k++)
                    {
                        var relation = entityHolder.AssigningRelations[k];

                        if (relation.RelationType == RelationType.OneToMany)
                        {
                            var nestedLoopConditionLabel = moveNextMethodIL.DefineLabel();
                            var startNestedLoopLabel = moveNextMethodIL.DefineLabel();

                            // Assign 0 to the nested iterator
                            moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
                            moveNextMethodIL.Emit(OpCodes.Stloc, iteratorLocal);
                            moveNextMethodIL.Emit(OpCodes.Br, nestedLoopConditionLabel);

                            // nested loop body

                            moveNextMethodIL.MarkLabel(startNestedLoopLabel);

                            // Assign the primary key to the foreign entity
                            moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                            moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                            moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                            moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetMethod("get_Item"));
                            moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                            moveNextMethodIL.Emit(OpCodes.Callvirt, relation.ForeignKeyColumn.PropertyInfo.GetSetMethod());

                            // nested loop iterator increment
                            moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                            moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                            moveNextMethodIL.Emit(OpCodes.Add);
                            moveNextMethodIL.Emit(OpCodes.Stloc, iteratorLocal);

                            // nested loop condition
                            moveNextMethodIL.MarkLabel(nestedLoopConditionLabel);
                            moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                            moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                            moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                            moveNextMethodIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetProperty("Count").GetGetMethod());
                            moveNextMethodIL.Emit(OpCodes.Blt, startNestedLoopLabel);
                        }
                        else if (relation.RelationType == RelationType.OneToOne)
                        {
                            moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                            moveNextMethodIL.Emit(OpCodes.Callvirt, relation.RightNavigationProperty.GetGetMethod());
                            moveNextMethodIL.Emit(OpCodes.Ldloc, primaryKeyLocal);
                            moveNextMethodIL.Emit(OpCodes.Callvirt, relation.ForeignKeyColumn.PropertyInfo.GetSetMethod());
                        }
                    }

                    // loop iterator increment
                    var tempIteratorLocal = moveNextMethodIL.DeclareLocal(intType);

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                    moveNextMethodIL.Emit(OpCodes.Stloc, tempIteratorLocal);
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, tempIteratorLocal);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_1);
                    moveNextMethodIL.Emit(OpCodes.Add);
                    moveNextMethodIL.Emit(OpCodes.Stfld, iteratorField);

                    // loop condition
                    moveNextMethodIL.MarkLabel(loopConditionLabel);
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, iteratorField);
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, entityListField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, entityListField.FieldType.GetProperty("Count").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Blt, startLoopBodyLabel);

                    // dispose data reader
                    afterAwaitLabel = moveNextMethodIL.DefineLabel();
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, GetDataReaderField());
                    moveNextMethodIL.Emit(OpCodes.Callvirt, npgsqlDataReaderType.GetMethod("DisposeAsync"));
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetValueTaskLocal());
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetValueTaskLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, valueTaskType.GetMethod("GetAwaiter"));
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetValueTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetValueTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, valueTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);
                    // Await Handler

                    // stateField = stateLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    moveNextMethodIL.Emit(OpCodes.Dup);
                    moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, GetValueTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Stfld, GetValueTaskAwaiterField());

                    // Call AwaitUnsafeOnCompleted
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetValueTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Call, asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(valueTaskAwaiterType, stateMachineTypeBuilder));
                    moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, GetValueTaskAwaiterField());
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetValueTaskAwaiterLocal());

                    // taskAwaiterField = default
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldflda, GetValueTaskAwaiterField());
                    moveNextMethodIL.Emit(OpCodes.Initobj, valueTaskAwaiterType);

                    // stateField = stateLocal = -1
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    moveNextMethodIL.Emit(OpCodes.Dup);
                    moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetValueTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, valueTaskAwaiterType.GetMethod("GetResult"));
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldnull);
                    moveNextMethodIL.Emit(OpCodes.Stfld, GetDataReaderField());
                }
                else
                {
                    var afterAwaitLabel = moveNextMethodIL.DefineLabel();

                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetMethod("ExecuteNonQueryAsync", Type.EmptyTypes)); // TODO: add Cancellation Token
                    moveNextMethodIL.Emit(OpCodes.Callvirt, taskIntType.GetMethod("GetAwaiter"));
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetIntTaskAwaiterLocal());

                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetIntTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, intTaskAwaiterType.GetProperty("IsCompleted").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Brtrue, afterAwaitLabel);

                    // Await Handler

                    // stateField = stateLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4, asyncStateIndex++);
                    moveNextMethodIL.Emit(OpCodes.Dup);
                    moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // taskAwaiterField = taskAwaiterLocal
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, GetIntTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Stfld, GetIntTaskAwaiterField());

                    // Call AwaitUnsafeOnCompleted
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetIntTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Call, asyncMethodBuilderIntType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(npgsqlTaskAwaiterType, stateMachineTypeBuilder));
                    moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

                    switchBuilder.MarkCase();

                    // taskAwaiterLocal = taskAwaiterField
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, GetIntTaskAwaiterField());
                    moveNextMethodIL.Emit(OpCodes.Stloc, GetIntTaskAwaiterLocal());

                    // taskAwaterField = default
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldflda, GetIntTaskAwaiterField());
                    moveNextMethodIL.Emit(OpCodes.Initobj, intTaskAwaiterType);

                    // stateField = stateLocal = -1
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4_M1);
                    moveNextMethodIL.Emit(OpCodes.Dup);
                    moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);
                    moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

                    // wait of the result from the TaskAwaiter
                    moveNextMethodIL.MarkLabel(afterAwaitLabel);
                    moveNextMethodIL.Emit(OpCodes.Ldloca, GetIntTaskAwaiterLocal());
                    moveNextMethodIL.Emit(OpCodes.Call, intTaskAwaiterType.GetMethod("GetResult"));
                    moveNextMethodIL.Emit(OpCodes.Pop);
                }

                moveNextMethodIL.MarkLabel(afterEntityInsertionLabel);
            }

            // Set the result to 0 TODO: Change that to the actual amount of inserted entities
            moveNextMethodIL.Emit(OpCodes.Ldc_I4_0);
            moveNextMethodIL.Emit(OpCodes.Stloc, resultLocal);
            moveNextMethodIL.Emit(OpCodes.Leave, endOfMethodLabel);

            moveNextMethodIL.BeginCatchBlock(exceptionType);

            // Store the exception in the exceptionLocal
            moveNextMethodIL.Emit(OpCodes.Stloc, exceptionLocal);

            // Assign -2 to the state field
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldc_I4, -2);
            moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

            // Set command builder to null
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldnull);
            moveNextMethodIL.Emit(OpCodes.Stfld, commandBuilderField);

            // Set command to null
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldnull);
            moveNextMethodIL.Emit(OpCodes.Stfld, commandField);

            setListsToNullGhostIL.WriteIL(moveNextMethodIL);

            // Set the exception to the method builder
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            moveNextMethodIL.Emit(OpCodes.Ldloc, exceptionLocal);
            moveNextMethodIL.Emit(OpCodes.Call, asyncMethodBuilderIntType.GetMethod("SetException"));
            moveNextMethodIL.Emit(OpCodes.Leave, retOfMethodLabel);

            moveNextMethodIL.EndExceptionBlock();

            moveNextMethodIL.MarkLabel(endOfMethodLabel);

            // Assign -2 to the state field
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldc_I4, -2);
            moveNextMethodIL.Emit(OpCodes.Stfld, stateField);

            //// Set command builder to null
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldnull);
            moveNextMethodIL.Emit(OpCodes.Stfld, commandBuilderField);

            // Set command to null
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldnull);
            moveNextMethodIL.Emit(OpCodes.Stfld, commandField);

            setListsToNullGhostIL.WriteIL(moveNextMethodIL);

            // Set the result to the method builder
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            moveNextMethodIL.Emit(OpCodes.Ldloc, resultLocal);
            moveNextMethodIL.Emit(OpCodes.Call, asyncMethodBuilderIntType.GetMethod("SetResult"));

            moveNextMethodIL.MarkLabel(retOfMethodLabel);
            moveNextMethodIL.Emit(OpCodes.Ret);

            #region StateMachine

            stateMachineTypeBuilder.DefineMethodOverride(moveNextMethod,
                asyncStateMachineType.GetMethod("MoveNext"));

            var setStateMachineMethod = stateMachineTypeBuilder.DefineMethod("SetStateMachine",
                MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual, null, new[] { asyncStateMachineType });
            var setStateMachineMethodIL = setStateMachineMethod.GetILGenerator();

            setStateMachineMethodIL.Emit(OpCodes.Ldarg_0);
            setStateMachineMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            setStateMachineMethodIL.Emit(OpCodes.Ldarg_1);
            setStateMachineMethodIL.Emit(OpCodes.Call, asyncMethodBuilderIntType.GetMethod("SetStateMachine"));
            setStateMachineMethodIL.Emit(OpCodes.Ret);

            stateMachineTypeBuilder.DefineMethodOverride(setStateMachineMethod,
                asyncStateMachineType.GetMethod("SetStateMachine"));

            var materializeMethod = inserterTypeBuilder.DefineMethod("InsertAsync",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, taskIntType,
                new[] { typeof(NpgsqlConnection), typeof(List<TEntity>) });

            materializeMethod.SetCustomAttribute(new CustomAttributeBuilder(
                        typeof(AsyncStateMachineAttribute).GetConstructor(new[] { typeof(Type)
        }),
                        new[] { stateMachineTypeBuilder }));

            var materializeMethodIL = materializeMethod.GetILGenerator();

            materializeMethodIL.DeclareLocal(stateMachineTypeBuilder);

            // Create and execute the StateMachine

            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Call,
                asyncMethodBuilderIntType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static));
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
            materializeMethodIL.Emit(OpCodes.Call,
               asyncMethodBuilderIntType.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance)
                    .MakeGenericMethod(stateMachineTypeBuilder));
            materializeMethodIL.Emit(OpCodes.Ldloca_S, (byte)0);
            materializeMethodIL.Emit(OpCodes.Ldflda, methodBuilderField);
            materializeMethodIL.Emit(OpCodes.Call, asyncMethodBuilderIntType.GetProperty("Task").GetGetMethod());

            materializeMethodIL.Emit(OpCodes.Ret);

            #endregion

            stateMachineTypeBuilder.CreateType();
            var inserterType = inserterTypeBuilder.CreateType();

            return (Func<NpgsqlConnection, List<TEntity>, Task<int>>)inserterType.GetMethod("InsertAsync").CreateDelegate(typeof(Func<NpgsqlConnection, List<TEntity>, Task<int>>));
        }
    }

    internal class SwitchBuilder
    {
        private int _labelIndex;

        private readonly Label[] _labels;
        private readonly ILGenerator _iLGenerator;

        internal SwitchBuilder(ILGenerator iLGenerator, int labelCount)
        {
            _iLGenerator = iLGenerator;
            _labels = new Label[labelCount];

            for (int i = 0; i < labelCount; i++)
            {
                _labels[i] = iLGenerator.DefineLabel();
            }
        }

        internal void MarkCase()
        {
            _iLGenerator.MarkLabel(_labels[_labelIndex++]);
        }

        internal Label[] GetLabels()
        {
            return _labels;
        }
    }

    internal static class ILGeneratorExtensions
    {
        internal static SwitchBuilder EmitSwitch(this ILGenerator ilGenerator, int labelCount)
        {
            var switchBuilder = new SwitchBuilder(ilGenerator, labelCount);

            ilGenerator.Emit(OpCodes.Switch, switchBuilder.GetLabels());

            return switchBuilder;
        }
    }

    internal class InsertionSourceCompiler
    {
        private readonly HashSet<int> _visitedEntities;
        private readonly HashSet<uint> _visitedRelations;
        private readonly LinkedList<EntityRelationHolder> _entityRelations;
        private readonly Dictionary<int, LinkedListNode<EntityRelationHolder>> _entityRelationLookup;

        internal InsertionSourceCompiler()
        {
            _visitedEntities = new HashSet<int>();
            _visitedRelations = new HashSet<uint>();
            _entityRelations = new LinkedList<EntityRelationHolder>();
            _entityRelationLookup = new Dictionary<int, LinkedListNode<EntityRelationHolder>>();
        }

        internal void Compile(Entity entity)
        {
            var entityId = RuntimeHelpers.GetHashCode(entity);

            _visitedEntities.Add(entityId);

            if (entity.Relations is null)
            {
                return;
            }

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                var leftHashCode = RuntimeHelpers.GetHashCode(relation.LeftEntity);
                var rightHashCode = RuntimeHelpers.GetHashCode(relation.RightEntity);

                if (relation.ForeignKeyLocation == ForeignKeyLocation.Left && relation.LeftNavigationProperty is { } && !_visitedRelations.Contains(relation.RelationId))
                {
                    if (!_entityRelationLookup.TryGetValue(rightHashCode, out var rightEntityHolder))
                    {
                        rightEntityHolder = new LinkedListNode<EntityRelationHolder>(new EntityRelationHolder(relation.RightEntity));

                        _entityRelationLookup.Add(rightHashCode, rightEntityHolder);

                        _entityRelations.AddLast(rightEntityHolder);
                    }

                    if (!_entityRelationLookup.TryGetValue(leftHashCode, out var leftEntityHolder))
                    {
                        leftEntityHolder = new LinkedListNode<EntityRelationHolder>(new EntityRelationHolder(relation.LeftEntity));

                        _entityRelationLookup.Add(leftHashCode, leftEntityHolder);

                        _entityRelations.AddAfter(rightEntityHolder, leftEntityHolder);
                    }

                    _visitedRelations.Add(relation.RelationId);

                    leftEntityHolder.Value.Relations.Add(relation);

                    rightEntityHolder.Value.AssigningRelations.Add(relation.RightEntity.Relations[relation.LeftEntity.EntityName]);
                }
            }

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                if (_visitedEntities.Contains(RuntimeHelpers.GetHashCode(relation.RightEntity)))
                    continue;

                Compile(relation.RightEntity);
            }
        }

        internal EntityRelationHolder[] GenerateSortedEntities()
        {
            var entities = new EntityRelationHolder[_entityRelations.Count];

            var index = 0;

            for (var entry = _entityRelations.First; entry is { }; entry = entry.Next)
            {
                entities[index++] = entry.Value;
            }

            return entities;
        }
    }

    internal class EntityRelationHolder
    {
        internal Entity Entity { get; }
        internal List<EntityRelation> Relations { get; }
        internal List<EntityRelation> AssigningRelations { get; }

        internal EntityRelationHolder(Entity entity)
        {
            Entity = entity;
            Relations = new List<EntityRelation>();
            AssigningRelations = new List<EntityRelation>();
        }
    }
}
