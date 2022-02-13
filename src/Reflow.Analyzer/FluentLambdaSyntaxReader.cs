using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Models.Definitions;
using Reflow.Analyzer.Shared;

namespace Reflow.Analyzer
{
    internal abstract class FluentLambdaSyntaxReader<T>
    {
        protected T Value { get; }
        protected SemanticModel SemanticModel { get; }

        private readonly IList<InvocationExpressionSyntax> _invocations;
        private readonly FluentCallDefinition _fluentCall;

        protected FluentLambdaSyntaxReader(T value, FluentCallDefinition fluentCall)
        {
            _fluentCall = fluentCall;
            _invocations = fluentCall.Invocations;
            SemanticModel = fluentCall.SemanticModel;
            Value = value;
        }

        internal void Evaluate()
        {
            var invocationSyntax = _invocations[0];
            var methodSymbol = (IMethodSymbol)SemanticModel.GetSymbolInfo(invocationSyntax).Symbol!;

            if (!methodSymbol.ConstructedFrom.IsReflowSymbol())
                throw new InvalidOperationException();

            if (
                !ValidateHead(
                    _fluentCall.LambdaSyntax,
                    methodSymbol,
                    invocationSyntax.ArgumentList.Arguments
                )
            )
            {
                throw new InvalidOperationException("Invalid head.");
            }

            for (var invocationIndex = 1; invocationIndex < _invocations.Count; invocationIndex++)
            {
                invocationSyntax = _invocations[invocationIndex];
                methodSymbol = (IMethodSymbol)SemanticModel.GetSymbolInfo(invocationSyntax).Symbol!;

                if (!methodSymbol.ConstructedFrom.IsReflowSymbol())
                    break;

                ReadTail(methodSymbol, invocationSyntax.ArgumentList.Arguments);
            }

            if (!ValidateTail())
            {
                throw new InvalidOperationException("Invalid tail.");
            }
        }

        protected void WithLinkData(ILambdaLinkData data)
        {
            _fluentCall.LambdaLink.Data = data;
        }

        protected abstract bool ValidateHead(
            LambdaExpressionSyntax lambdaSyntax,
            IMethodSymbol methodSymbol,
            SeparatedSyntaxList<ArgumentSyntax> arguments
        );

        protected abstract void ReadTail(
            IMethodSymbol methodSymbol,
            SeparatedSyntaxList<ArgumentSyntax> arguments
        );

        protected virtual bool ValidateTail()
        {
            return true;
        }
    }
}
