using Microsoft.CodeAnalysis;
using Reflow.Internal.Analyzer.Emitters;

namespace Reflow.Internal.Analyzer
{
    [Generator]
    public class OrderingSourceGenerators : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddNamedSource(
                "OrderableGeneratorAttribute",
                OrderableGeneratorAttributeEmitter.Emit()
            );
        }
    }
}
