using System.Reflection;
using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
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

        void IILBaseInst.WriteIL(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(_opCode, _value);
        }
    }
}
