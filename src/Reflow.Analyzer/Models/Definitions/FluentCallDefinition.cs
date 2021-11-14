using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Models.Definitions
{
    internal class FluentCallDefinition
    {
        internal SemanticModel SemanticModel { get; }
        internal LambdaExpressionSyntax LambdaSyntax { get; }
        internal List<InvocationExpressionSyntax> Invocations { get; }
        internal LambdaLink LambdaLink { get; }

        internal FluentCallDefinition(
            SemanticModel semanticModel,
            LambdaExpressionSyntax lambdaSyntax,
            List<InvocationExpressionSyntax> invocations,
            LambdaLink lambdaLink
        )
        {
            SemanticModel = semanticModel;
            LambdaSyntax = lambdaSyntax;
            Invocations = invocations;
            LambdaLink = lambdaLink;
        }
    }
}
