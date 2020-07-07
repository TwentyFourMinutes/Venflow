using System.Reflection.Emit;

namespace Venflow.Dynamic
{
    internal static class ILGeneratorExtensions
    {
        internal static SwitchBuilder EmitSwitch(this ILGenerator ilGenerator, int labelCount)
        {
            var switchBuilder = new SwitchBuilder(ilGenerator, labelCount);

            ilGenerator.Emit(OpCodes.Switch, switchBuilder.GetLabels());

            return switchBuilder;
        }
    }
}
