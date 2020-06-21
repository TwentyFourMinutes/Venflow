using System.Reflection;
using System.Reflection.Emit;

namespace Venflow.Dynamic
{

    internal struct ILMethodInfo : IILBaseInst
    {
        private readonly OpCode _opCode;
        private readonly MethodInfo _value;

        internal ILMethodInfo(OpCode opCode, MethodInfo value)
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