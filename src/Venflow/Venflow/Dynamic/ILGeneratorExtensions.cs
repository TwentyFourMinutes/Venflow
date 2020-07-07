using System.Reflection.Emit;

namespace Venflow.Dynamic
{
    internal static class ILGeneratorExtensions
    {
        internal static ILSwitchBuilder EmitSwitch(this ILGenerator ilGenerator, int labelCount)
        {
            var switchBuilder = new ILSwitchBuilder(ilGenerator, labelCount);

            ilGenerator.Emit(OpCodes.Switch, switchBuilder.GetLabels());

            return switchBuilder;
        }
    }
}
