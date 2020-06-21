using System.Reflection;
using System.Reflection.Emit;

namespace Venflow.Dynamic
{

    internal struct ILFieldInfo : IILBaseInst
    {
        private readonly OpCode _opCode;
        private readonly FieldInfo _value;

        internal ILFieldInfo(OpCode opCode, FieldInfo value)
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