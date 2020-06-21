using System.Reflection.Emit;

namespace Venflow.Dynamic
{

    internal struct ILInst : IILBaseInst
    {
        private readonly OpCode _opCode;

        internal ILInst(OpCode opCode)
        {
            _opCode = opCode;
        }

        public void WriteIL(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(_opCode);
        }
    }
}