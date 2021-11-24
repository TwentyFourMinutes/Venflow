using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Sections
{
    internal abstract class BulkFluentSyntaxReader<T>
    {
        protected T Value { get; }
        protected SemanticModel SemanticModel { get; }

        private readonly BlockSyntax _blockSyntax;

        protected BulkFluentSyntaxReader(
            T value,
            SemanticModel semanticModel,
            BlockSyntax blockSyntax
        )
        {
            _blockSyntax = blockSyntax;
            SemanticModel = semanticModel;
            Value = value;
        }

        internal void Evaluate()
        {
            var statements = _blockSyntax.Statements;

            var invocations =
                new List<(IMethodSymbol MethodSymbol, SeparatedSyntaxList<ArgumentSyntax> Arguments)>();

            for (var statementIndex = 0; statementIndex < statements.Count; statementIndex++)
            {
                var statement = statements[statementIndex];

                if (
                    statement is not ExpressionStatementSyntax expressionSyntax
                    || expressionSyntax.Expression
                        is not InvocationExpressionSyntax rootInvocationSyntax
                )
                    continue;

                ExpressionSyntax expression = rootInvocationSyntax;

                while (true)
                {
                    if (expression is MemberAccessExpressionSyntax childMemberAccessSyntax)
                    {
                        expression = childMemberAccessSyntax.Expression;
                    }
                    else if (expression is InvocationExpressionSyntax childInvocationSyntax)
                    {
                        var methodSymbol = (IMethodSymbol)SemanticModel!.GetSymbolInfo(
                            childInvocationSyntax
                        ).Symbol!;

                        if (!methodSymbol.ConstructedFrom.IsReflowSymbol())
                            throw new InvalidOperationException();

                        invocations.Add(
                            (methodSymbol, childInvocationSyntax.ArgumentList.Arguments)
                        );

                        expression = childInvocationSyntax.Expression;
                    }
                    else if (expression is IdentifierNameSyntax)
                    {
                        break;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                var invocation = invocations[invocations.Count - 1];

                if (!ValidateHead(invocation.MethodSymbol, invocation.Arguments))
                {
                    throw new InvalidOperationException("Invalid head.");
                }

                for (
                    var invocationSyntaxIndex = invocations.Count - 2;
                    invocationSyntaxIndex >= 0;
                    invocationSyntaxIndex--
                )
                {
                    invocation = invocations[invocationSyntaxIndex];

                    ReadTail(invocation.MethodSymbol, invocation.Arguments);
                }

                if (!ValidateTail())
                {
                    throw new InvalidOperationException("Invalid tail.");
                }

                invocations.Clear();
            }
        }

        protected abstract bool ValidateHead(
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
