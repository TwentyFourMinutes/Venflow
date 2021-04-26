using System.Reflection.Emit;

namespace Venflow.Dynamic.IL
{
    internal interface IILBaseInst
    {
        void WriteIL(ILGenerator ilGenerator);
    }
}