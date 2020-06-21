using System.Reflection;
using System.Reflection.Emit;

namespace Venflow.Dynamic
{

    internal struct ILConstructorInfo : IILBaseInst
    {
        private readonly OpCode _opCode;
        private readonly ConstructorInfo _value;

        internal ILConstructorInfo(OpCode opCode, ConstructorInfo value)
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