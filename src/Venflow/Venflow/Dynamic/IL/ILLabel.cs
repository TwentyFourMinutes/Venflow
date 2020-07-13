using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
{

    internal struct ILLabel : IILBaseInst
    {
        private readonly OpCode _opCode;
        private readonly Label _label;

        internal ILLabel(OpCode opCode, Label label)
        {
            _opCode = opCode;
            _label = label;
        }

        public void WriteIL(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(_opCode, _label);
        }
    }
}