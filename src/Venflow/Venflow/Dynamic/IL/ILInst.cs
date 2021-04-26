using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
{
    internal struct ILInst : IILBaseInst
    {
        private readonly OpCode _opCode;

        internal ILInst(OpCode opCode)
        {
            _opCode = opCode;
        }

        void IILBaseInst.WriteIL(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(_opCode);
        }
    }
}