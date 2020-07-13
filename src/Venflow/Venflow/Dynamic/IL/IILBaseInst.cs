using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
{
    internal interface IILBaseInst
    {
        public void WriteIL(ILGenerator ilGenerator);
    }
}