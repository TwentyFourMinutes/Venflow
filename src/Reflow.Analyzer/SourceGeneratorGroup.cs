using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer
{
    internal abstract class SourceGeneratorGroup<TData> : ISourceGenerator
        where TData : class, new()
    {
        protected IList<IGroupableSourceGenerator<TData>> SourceGenerators { get; }

        private TData _data = null!;

        protected SourceGeneratorGroup()
        {
            SourceGenerators = new List<IGroupableSourceGenerator<TData>>();
        }

        void ISourceGenerator.Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(
                () =>
                {
                    var syntaxReceivers = new ISyntaxContextReceiver[SourceGenerators.Count];

                    for (
                        var sourceGeneratorIndex = 0;
                        sourceGeneratorIndex < SourceGenerators.Count;
                        sourceGeneratorIndex++
                    )
                    {
                        syntaxReceivers[sourceGeneratorIndex] = SourceGenerators[
                            sourceGeneratorIndex
                        ].Initialize();
                    }

                    return new SyntaxContextReceiver(syntaxReceivers);
                }
            );
        }

        void ISourceGenerator.Execute(GeneratorExecutionContext context)
        {
            _data = new TData();

            var syntaxReceiver = (SyntaxContextReceiver)context.SyntaxContextReceiver!;

            for (
                var sourceGeneratorIndex = 0;
                sourceGeneratorIndex < SourceGenerators.Count;
                sourceGeneratorIndex++
            )
            {
                SourceGenerators[sourceGeneratorIndex].Execute(
                    context,
                    syntaxReceiver.SyntaxReceivers[sourceGeneratorIndex],
                    _data
                );
            }
        }

        private class SyntaxContextReceiver : ISyntaxContextReceiver
        {
            internal ISyntaxContextReceiver[] SyntaxReceivers { get; }

            internal SyntaxContextReceiver(ISyntaxContextReceiver[] syntaxReceivers)
            {
                SyntaxReceivers = syntaxReceivers;
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                for (
                    var syntaxReceiverIndex = 0;
                    syntaxReceiverIndex < SyntaxReceivers.Length;
                    syntaxReceiverIndex++
                )
                {
                    SyntaxReceivers[syntaxReceiverIndex].OnVisitSyntaxNode(context);
                }
            }
        }
    }

    internal interface IGroupableSourceGenerator<TData> where TData : class
    {
        ISyntaxContextReceiver Initialize();
        void Execute(
            GeneratorExecutionContext context,
            ISyntaxContextReceiver contextReceiver,
            TData data
        );
    }
}
