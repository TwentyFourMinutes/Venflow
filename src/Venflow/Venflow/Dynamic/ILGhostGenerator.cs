using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Venflow.Dynamic
{
    internal class ILGhostGenerator
    {
        private IILBaseInst[] _ilInstructions;
        private int _index;

        internal ILGhostGenerator()
        {
            _ilInstructions = new IILBaseInst[16];
        }

        internal void EnsureCapacity()
        {
            if (_ilInstructions.Length - 1 != _index)
                return;

            var tempArr = new IILBaseInst[_ilInstructions.Length * 2];

            Array.Copy(_ilInstructions, 0, tempArr, 0, _ilInstructions.Length - 1);

            _ilInstructions = tempArr;
        }

        internal void Emit(OpCode opCode)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILInst(opCode);
        }

        internal void Emit(OpCode opCode, sbyte value)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILSbyteInst(opCode, value);
        }

        internal void Emit(OpCode opCode, int value)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILIntInst(opCode, value);
        }

        internal void Emit(OpCode opCode, MethodInfo method)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILMethodInfo(opCode, method);
        }

        internal void Emit(OpCode opCode, FieldInfo field)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILFieldInfo(opCode, field);
        }

        internal void Emit(OpCode opCode, LocalBuilder local)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILLocalBuilder(opCode, local);
        }

        internal void Emit(OpCode opCode, ConstructorInfo constructor)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILConstructorInfo(opCode, constructor);
        }

        internal void Emit(OpCode opCode, Label label)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILLabel(opCode, label);
        }


        internal void MarkLabel(Label label)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILMarkLabel(label);
        }

        internal void WriteIL(ILGenerator ilGenerator)
        {
            for (var i = 0; i < _index; i++)
            {
                _ilInstructions[i].WriteIL(ilGenerator);
            }
        }
    }
}