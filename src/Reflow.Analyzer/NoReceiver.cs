using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer
{
    internal class NoReceiver : ISyntaxContextReceiver
    {
        void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context) { }
    }
}
