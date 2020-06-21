using System.Reflection.Emit;

namespace Venflow.Dynamic
{
    internal interface IILBaseInst
    {
        public void WriteIL(ILGenerator ilGenerator);
    }
}