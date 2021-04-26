using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
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


        internal void Emit(OpCode opCode, FieldInfo field)
        {
            EnsureCapacity();

            _ilInstructions[_index++] = new ILFieldInfo(opCode, field);
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