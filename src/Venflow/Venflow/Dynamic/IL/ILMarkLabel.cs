using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
{
    internal struct ILMarkLabel : IILBaseInst
    {
        private readonly Label _label;

        internal ILMarkLabel(Label label)
        {
            _label = label;
        }

        public void WriteIL(ILGenerator ilGenerator)
        {
            ilGenerator.MarkLabel(_label);
        }
    }
}