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
            var asyncMethodBuilderType = typeof(AsyncTaskMethodBuilder<>).MakeGenericType(intType);
            var npgsqlDataReaderType = typeof(NpgsqlDataReader);
            var taskAwaiterType = typeof(TaskAwaiter<bool>);
            var taskBoolType = typeof(Task<bool>);
            var exceptionType = typeof(Exception);

            var inserterTypeBuilder = TypeFactory.GetNewInserterBuilder(_entity.EntityName,
                TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Abstract |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            var stateMachineTypeBuilder = inserterTypeBuilder.DefineNestedType("StateMachine",
                TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, typeof(ValueType),
                new[] { asyncStateMachineType });

            // Required StateMachine Fields
            var stateField = stateMachineTypeBuilder.DefineField("_state", intType, FieldAttributes.Public);
            var methodBuilderField = stateMachineTypeBuilder.DefineField("_builder", asyncMethodBuilderType, FieldAttributes.Public);
            var dataReaderField = stateMachineTypeBuilder.DefineField("_dataReader", npgsqlDataReaderType, FieldAttributes.Public);
            var taskAwaiterField = stateMachineTypeBuilder.DefineField("_awaiter", taskAwaiterType, FieldAttributes.Private);

            // Custom Fields
            var rootEntityListField = stateMachineTypeBuilder.DefineField("_" + rootEntity.EntityName + "List", rootEntityListType, FieldAttributes.Public);

            var connectionField = stateMachineTypeBuilder.DefineField("_connection", typeof(NpgsqlConnection), FieldAttributes.Public);
            var commandBuilderField = stateMachineTypeBuilder.DefineField("_commandBuilder", typeof(StringBuilder), FieldAttributes.Private);
            var commandField = stateMachineTypeBuilder.DefineField("_command", typeof(NpgsqlCommand), FieldAttributes.Private);

            var moveNextMethod = stateMachineTypeBuilder.DefineMethod("MoveNext",
                MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual);
            var moveNextMethodIL = moveNextMethod.GetILGenerator();

            var stateLocal = moveNextMethodIL.DeclareLocal(intType);
            var resultLocal = moveNextMethodIL.DeclareLocal(intType);
            var awaiterLocal = moveNextMethodIL.DeclareLocal(taskAwaiterType);
            var exceptionLocal = moveNextMethodIL.DeclareLocal(exceptionType);

            var endOfMethodLabel = moveNextMethodIL.DefineLabel();

            // Assign the local state from the local field => state = _state;
            moveNextMethodIL.Emit(OpCodes.Ldarg_0);
            moveNextMethodIL.Emit(OpCodes.Ldfld, stateField);
            moveNextMethodIL.Emit(OpCodes.Stloc, stateLocal);

            var switchLabels = new List<Label>();

            var defaultEntitySeperationGhostIL = new ILGhostGenerator();
            var entityListAssignmentsGhostIL = new ILGhostGenerator();

            var entityListDict = new Dictionary<long, FieldBuilder>();
            var knownEntities = new ObjectIDGenerator();

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

                    var entityListField = stateMachineTypeBuilder.DefineField("_" + entity.EntityName + relation.RightEntity.EntityName + "List", relation.LeftNavigationProperty.PropertyType, FieldAttributes.Private);

                    var id = knownEntities.GetId(relation.RightEntity, out var isKnown);

                    if (!isKnown)
                    {
                        entityListDict.Add(id, entityListField);
                    }

                    // Instantiate entityListField
                    entityListAssignmentsGhostIL.Emit(OpCodes.Ldarg_0);
                    entityListAssignmentsGhostIL.Emit(OpCodes.Newobj, relation.LeftNavigationProperty.PropertyType.GetConstructor(Type.EmptyTypes));
                    entityListAssignmentsGhostIL.Emit(OpCodes.Stfld, entityListField);

                    if (relation.RelationType == RelationType.ManyToOne)
                    {
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

                        // Assign the entities to the local list
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, entityListField);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetMethod("AddRange"));
                        defaultEntitySeperationGhostIL.MarkLabel(afterListBodyLabel);
                    }
                    else
                    {
                        var visitedEntitiesField = stateMachineTypeBuilder.DefineField("_visited" + entity.EntityName + relation.RightEntity.EntityName, typeof(ObjectIDGenerator), FieldAttributes.Private);

                        // Instantiate visitedEntitiesField
                        entityListAssignmentsGhostIL.Emit(OpCodes.Ldarg_0);
                        entityListAssignmentsGhostIL.Emit(OpCodes.Newobj, typeof(ObjectIDGenerator).GetConstructor(Type.EmptyTypes));
                        entityListAssignmentsGhostIL.Emit(OpCodes.Stfld, visitedEntitiesField);

                        var isVisitedEntityLocal = moveNextMethodIL.DeclareLocal(typeof(bool));

                        var afterCheckBodyLabel = moveNextMethodIL.DefineLabel();

                        // Check if navigation property is set
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Brfalse, afterCheckBodyLabel);

                        // Check if instance was already visited
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, visitedEntitiesField);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloca, isVisitedEntityLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, typeof(ObjectIDGenerator).GetMethod("GetId"));
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Pop);

                        // Check if instance hasn't been visited yet
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, isVisitedEntityLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Brtrue, afterCheckBodyLabel);

                        // Assign entity to the list
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldarg_0);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldfld, entityListField);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.GetGetMethod());
                        defaultEntitySeperationGhostIL.Emit(OpCodes.Callvirt, relation.LeftNavigationProperty.PropertyType.GetMethod("Add"));

                        defaultEntitySeperationGhostIL.MarkLabel(afterCheckBodyLabel);
                    }

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
            moveNextMethodIL.Emit(OpCodes.Ldloc, stateLocal);
            moveNextMethodIL.Emit(OpCodes.Switch, switchLabels.ToArray());

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
            moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Connection").GetSetMethod());

            for (int i = 0; i < entities.Length; i++)
            {
                var entityHolder = entities[i];

                var isRoot = object.ReferenceEquals(entityHolder.Entity, rootEntity);

                var entityListField = entityListDict[knownEntities.HasId(entityHolder.Entity, out _)];

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

                var stringBuilder = new StringBuilder();

                var skipPrimaryKey = ((IPrimaryEntityColumn)entityHolder.Entity.GetPrimaryColumn()).IsServerSideGenerated;

                stringBuilder.Append("INSERT INTO ")
                             .Append(entityHolder.Entity.EntityName)
                             .Append(" (")
                             .Append(skipPrimaryKey ? entityHolder.Entity.NonPrimaryColumnListString : entityHolder.Entity.ColumnListString)
                             .Append(") VALUES ");

                // Append base Insert Command to command builder
                moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                moveNextMethodIL.Emit(OpCodes.Ldstr, stringBuilder.ToString());
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));

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
                    moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorLocal);
                    moveNextMethodIL.Emit(OpCodes.Call, intType.GetMethod("ToString", new[] { intType }));
                    moveNextMethodIL.Emit(OpCodes.Call, intType.GetMethod("Concat", new[] { typeof(string), typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Stloc, placeholderLocal);

                    // Write placeholder to the command builder => "(@Name(n)), "
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandBuilderField);
                    moveNextMethodIL.Emit(OpCodes.Ldc_I4, (int)'"');
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(char) }));
                    moveNextMethodIL.Emit(OpCodes.Ldstr, placeholderLocal);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Ldstr, columnCount == k ? "\"), " : "\", ");
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("Append", new[] { typeof(string) }));
                    moveNextMethodIL.Emit(OpCodes.Pop);

                    // Create new parameter with placeholder and add it to the parameter list
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Parameters").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Ldloc, placeholderLocal);
                    moveNextMethodIL.Emit(OpCodes.Ldloc, iteratorElementLocal);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, column.PropertyInfo.GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(column.PropertyInfo.PropertyType).GetConstructor(new[] { typeof(string), column.PropertyInfo.PropertyType }));
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("Parameters").PropertyType.GetMethod("Add"));
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
                    moveNextMethodIL.Emit(OpCodes.Ldstr, "RETURNING \"");
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
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandBuilderField.FieldType.GetMethod("ToString")); // maybe object.ToString instead
                moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetProperty("CommandText").GetSetMethod());

                // Execute Reader and use SingleRow if only one entity
                var beforeElseLabel = moveNextMethodIL.DefineLabel();
                var afterElseLabel = moveNextMethodIL.DefineLabel();

                if (skipPrimaryKey)
                {
                    //TODO Use ExecuteScalar instead of Singlerow
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
                    moveNextMethodIL.Emit(OpCodes.Callvirt, npgsqlDataReaderType.GetMethod("GetAwaiter"));
                    moveNextMethodIL.Emit(OpCodes.Stloc, taskAwaiterField);
                }
                else
                {
                    moveNextMethodIL.Emit(OpCodes.Ldarg_0);
                    moveNextMethodIL.Emit(OpCodes.Ldfld, commandField);
                    moveNextMethodIL.Emit(OpCodes.Callvirt, commandField.FieldType.GetMethod("ExecuteNonQueryAsync", Type.EmptyTypes)); // TODO: add Cancellation Token
                    moveNextMethodIL.Emit(OpCodes.Callvirt, npgsqlDataReaderType.GetMethod("GetAwaiter"));
                    moveNextMethodIL.Emit(OpCodes.Stloc, taskAwaiterField);

                    moveNextMethodIL.Emit(OpCodes.Ldloca, taskAwaiterField);
                    moveNextMethodIL.Emit(OpCodes.Call, taskAwaiterField.FieldType.GetProperty("IsCompleted").GetGetMethod());
                    moveNextMethodIL.Emit(OpCodes.Brtrue, taskAwaiterField.FieldType.GetProperty("IsCompleted").GetGetMethod());
                }
            }

            moveNextMethodIL.MarkLabel(endOfMethodLabel);

            // TOOD: set stuff to null

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
            setStateMachineMethodIL.Emit(OpCodes.Call, asyncMethodBuilderType.GetMethod("SetStateMachine"));
            setStateMachineMethodIL.Emit(OpCodes.Ret);

            stateMachineTypeBuilder.DefineMethodOverride(setStateMachineMethod,
                asyncStateMachineType.GetMethod("SetStateMachine"));

            var materializeMethod = inserterTypeBuilder.DefineMethod("InsertAsync",
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

            #endregion

            stateMachineTypeBuilder.CreateType();
            var inserterType = inserterTypeBuilder.CreateType();

            return (Func<NpgsqlConnection, List<TEntity>, Task<int>>)inserterType.GetMethod("InsertAsync").CreateDelegate(typeof(Func<NpgsqlConnection, List<TEntity>, Task<int>>));
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
            // TODO: Use ObjectIdGenerator
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

        internal EntityRelationHolder(Entity entity)
        {
            Entity = entity;
            Relations = new List<EntityRelation>();
        }
    }
}
