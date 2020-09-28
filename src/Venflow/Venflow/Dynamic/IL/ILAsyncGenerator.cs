using System;
using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
{
    // TODO: Add ConfigureAwait(false) to all calls
    internal class ILAsyncGenerator
    {
        private int _awaiterIndex = -1;

        private readonly ILGenerator _ilGenerator;
        private readonly ILSwitchBuilder _ilSwitchBuilder;

        private readonly Type _stateMachineType;

        private readonly FieldBuilder _methodBuilderField;

        private readonly FieldBuilder _stateField;
        private readonly LocalBuilder _stateLocal;

        private readonly Label _returnOfMethodLabel;

        internal ILAsyncGenerator(ILGenerator ilGenerator, ILSwitchBuilder ilSwitchBuilder, FieldBuilder methodBuilderField, FieldBuilder stateField, LocalBuilder stateLocal, Label returnOfMethodLabel, Type stateMachineType)
        {
            _ilGenerator = ilGenerator;
            _ilSwitchBuilder = ilSwitchBuilder;
            _methodBuilderField = methodBuilderField;
            _stateField = stateField;
            _stateLocal = stateLocal;
            _returnOfMethodLabel = returnOfMethodLabel;
            _stateMachineType = stateMachineType;
        }

        internal void WriteAsyncMethodAwaiter(Type returnType, LocalBuilder taskAwaiterLocal, FieldBuilder taskAwaiterField)
        {
            var afterAwaitLabel = _ilGenerator.DefineLabel();

            _ilGenerator.Emit(OpCodes.Callvirt, returnType.GetMethod("GetAwaiter"));
            _ilGenerator.Emit(OpCodes.Stloc, taskAwaiterLocal);
            _ilGenerator.Emit(OpCodes.Ldloca, taskAwaiterLocal);
            _ilGenerator.Emit(OpCodes.Call, taskAwaiterLocal.LocalType.GetProperty("IsCompleted").GetGetMethod());
            _ilGenerator.Emit(OpCodes.Brtrue, afterAwaitLabel);

            // await handler

            // stateField = stateLocal
            _ilGenerator.Emit(OpCodes.Ldarg_0);

            if (++_awaiterIndex < sbyte.MaxValue)
                _ilGenerator.Emit(OpCodes.Ldc_I4_S, (sbyte) _awaiterIndex);
            else
                _ilGenerator.Emit(OpCodes.Ldc_I4, _awaiterIndex);

            _ilGenerator.Emit(OpCodes.Dup);
            _ilGenerator.Emit(OpCodes.Stloc, _stateLocal);
            _ilGenerator.Emit(OpCodes.Stfld, _stateField);

            // taskAwaiterField = taskAwaiterLocal
            _ilGenerator.Emit(OpCodes.Ldarg_0);
            _ilGenerator.Emit(OpCodes.Ldloc, taskAwaiterLocal);
            _ilGenerator.Emit(OpCodes.Stfld, taskAwaiterField);

            // call AwaitUnsafeOnCompleted
            _ilGenerator.Emit(OpCodes.Ldarg_0);
            _ilGenerator.Emit(OpCodes.Ldflda, _methodBuilderField);
            _ilGenerator.Emit(OpCodes.Ldloca, taskAwaiterLocal);
            _ilGenerator.Emit(OpCodes.Ldarg_0);
            _ilGenerator.Emit(OpCodes.Call, _methodBuilderField.FieldType.GetMethod("AwaitUnsafeOnCompleted").MakeGenericMethod(taskAwaiterLocal.LocalType, _stateMachineType));
            _ilGenerator.Emit(OpCodes.Leave, _returnOfMethodLabel);

            _ilSwitchBuilder.MarkCase();

            // taskAwaiterLocal = taskAwaiterField
            _ilGenerator.Emit(OpCodes.Ldarg_0);
            _ilGenerator.Emit(OpCodes.Ldfld, taskAwaiterField);
            _ilGenerator.Emit(OpCodes.Stloc, taskAwaiterLocal);

            // taskAwaiterField = default
            _ilGenerator.Emit(OpCodes.Ldarg_0);
            _ilGenerator.Emit(OpCodes.Ldflda, taskAwaiterField);
            _ilGenerator.Emit(OpCodes.Initobj, taskAwaiterField.FieldType);

            // stateField = stateLocal = -1
            _ilGenerator.Emit(OpCodes.Ldarg_0);
            _ilGenerator.Emit(OpCodes.Ldc_I4_M1);
            _ilGenerator.Emit(OpCodes.Dup);
            _ilGenerator.Emit(OpCodes.Stloc, _stateLocal);
            _ilGenerator.Emit(OpCodes.Stfld, _stateField);

            // wait of the result from the TaskAwaiter
            _ilGenerator.MarkLabel(afterAwaitLabel);
            _ilGenerator.Emit(OpCodes.Ldloca, taskAwaiterLocal);
            _ilGenerator.Emit(OpCodes.Call, taskAwaiterLocal.LocalType.GetMethod("GetResult"));
        }

        internal void WriteAsyncValueTaskMethodAwaiter(LocalBuilder valueTaskLocal, LocalBuilder valueTaskAwaiterLocal, FieldBuilder valueTaskAwaiterField)
        {
            _ilGenerator.Emit(OpCodes.Stloc, valueTaskLocal);
            _ilGenerator.Emit(OpCodes.Ldloca, valueTaskLocal);

            WriteAsyncMethodAwaiter(valueTaskLocal.LocalType, valueTaskAwaiterLocal, valueTaskAwaiterField);
        }
    }
}
