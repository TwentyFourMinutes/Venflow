using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
{

    internal struct ILSbyteInst : IILBaseInst
    {
        private readonly OpCode _opCode;
        private readonly sbyte _value;

        public ILSbyteInst(OpCode opCode, sbyte value)
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