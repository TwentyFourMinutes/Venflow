using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Emitters;

namespace Reflow.Analyzer.Sections
{
    internal class DeferredEmitterSection : GeneratorSection<OperationEmitterSection>
    {
        protected override NoData Execute(
            GeneratorExecutionContext context,
            NoReceiver syntaxReceiver,
            OperationEmitterSection previous
        )
        {
            AddSource(
                "DatabaseInstantiater",
                DatabaseConfigurationEmitter.Emit(GetPrevious<DatabaseConfigurationSection>().Data)
            );

            return default;
        }
    }
}
