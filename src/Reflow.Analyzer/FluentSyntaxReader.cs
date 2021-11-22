using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer
{
    internal abstract class FluentSyntaxReader<T> where T : new()
    {
        protected T Value { get; }
        protected SemanticModel SemanticModel { get; }

        private readonly IList<InvocationExpressionSyntax> _invocations;
        private readonly FluentCallDefinition _fluentCall;

        protected FluentSyntaxReader(FluentCallDefinition fluentCall)
        {
            _fluentCall = fluentCall;
            _invocations = fluentCall.Invocations;
            SemanticModel = fluentCall.SemanticModel;
            Value = new();
        }

        internal T Evaluate()
        {
            var invocation = _invocations[0];
            if (
                !ValidateHead(
                    _fluentCall.LambdaSyntax,
                    ((MemberAccessExpressionSyntax)invocation.Expression).Name.Identifier.Text,
                    invocation.ArgumentList
                )
            )
            {
                throw new InvalidOperationException("Invalid head.");
            }

            for (var invocationIndex = 1; invocationIndex < _invocations.Count; invocationIndex++)
            {
                invocation = _invocations[invocationIndex];
                ReadTail(
                    ((MemberAccessExpressionSyntax)invocation.Expression).Name.Identifier.Text,
                    invocation.ArgumentList
                );
            }

            if (!ValidateTail())
            {
                throw new InvalidOperationException("Invalid tail.");
            }

            return Value;
        }

        protected void WithLinkData(ILambdaLinkData data)
        {
            _fluentCall.LambdaLink.Data = data;
        }

        protected abstract bool ValidateHead(
            LambdaExpressionSyntax lambdaSyntax,
            string name,
            ArgumentListSyntax list
        );

        protected abstract void ReadTail(string name, ArgumentListSyntax list);

        protected virtual bool ValidateTail()
        {
            return true;
        }
    }
}
