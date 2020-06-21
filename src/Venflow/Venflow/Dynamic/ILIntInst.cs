using System.Reflection.Emit;

namespace Venflow.Dynamic
{

    internal struct ILIntInst : IILBaseInst
    {
        private readonly OpCode _opCode;
        private readonly int _value;

        internal ILIntInst(OpCode opCode, int value)
        {
            _opCode = opCode;
            _value = value;
        }

        public void WriteIL(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(_opCode, _value);
        }
    }
}