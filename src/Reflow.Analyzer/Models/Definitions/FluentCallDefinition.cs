using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Models.Definitions
{
    internal class FluentCallDefinition
    {
        internal SemanticModel SemanticModel { get; }
        internal LambdaExpressionSyntax LambdaSyntax { get; }
        internal List<InvocationExpressionSyntax> Invocations { get; }
        internal LambdaLinkDefinition LambdaLink { get; }

        internal FluentCallDefinition(
            SemanticModel semanticModel,
            LambdaExpressionSyntax lambdaSyntax,
            List<InvocationExpressionSyntax> invocations,
            LambdaLinkDefinition lambdaLink
        )
        {
            SemanticModel = semanticModel;
            LambdaSyntax = lambdaSyntax;
            Invocations = invocations;
            LambdaLink = lambdaLink;
        }
    }
}
