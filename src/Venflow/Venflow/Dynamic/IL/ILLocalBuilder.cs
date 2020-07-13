using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
{

    internal struct ILLocalBuilder : IILBaseInst
    {
        private readonly OpCode _opCode;
        private readonly LocalBuilder _value;

        internal ILLocalBuilder(OpCode opCode, LocalBuilder value)
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