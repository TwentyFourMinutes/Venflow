using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Sections
{
    internal partial class Entity
    {
        private abstract class BulkFluentReader<T>
        {
            protected T Value { get; }
            protected SemanticModel SemanticModel { get; }

            private readonly BlockSyntax _blockSyntax;

            protected BulkFluentReader(
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

                var invocationSyntaxis = new List<InvocationExpressionSyntax>();

                for (var statementIndex = 0; statementIndex < statements.Count; statementIndex++)
                {
                    var statement = statements[statementIndex];

                    if (
                        statement is not ExpressionStatementSyntax expressionSyntax
                        || expressionSyntax.Expression
                            is not InvocationExpressionSyntax rootInvocationSyntax
                    )
                        continue;

                    invocationSyntaxis.Add(rootInvocationSyntax);

                    var expression = rootInvocationSyntax.Expression;

                    while (true)
                    {
                        if (expression is MemberAccessExpressionSyntax childMemberAccessSyntax)
                        {
                            expression = childMemberAccessSyntax.Expression;
                        }
                        else if (expression is InvocationExpressionSyntax childInvocationSyntax)
                        {
                            var symbol = (IMethodSymbol)SemanticModel!.GetSymbolInfo(
                                childInvocationSyntax
                            ).Symbol!;

                            if (!symbol.ConstructedFrom.IsReflowSymbol())
                                throw new InvalidOperationException();

                            invocationSyntaxis.Add(childInvocationSyntax);

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

                    var invocation = invocationSyntaxis[invocationSyntaxis.Count - 1];

                    if (
                        !ValidateHead(
                            (
                                (MemberAccessExpressionSyntax)invocation.Expression
                            ).Name.Identifier.Text,
                            invocation.ArgumentList.Arguments
                        )
                    )
                    {
                        throw new InvalidOperationException("Invalid head.");
                    }

                    for (
                        var invocationSyntaxIndex = invocationSyntaxis.Count - 2;
                        invocationSyntaxIndex >= 0;
                        invocationSyntaxIndex--
                    )
                    {
                        var invocationSyntax = invocationSyntaxis[invocationSyntaxIndex];

                        ReadTail(
                            (
                                (MemberAccessExpressionSyntax)invocationSyntax.Expression
                            ).Name.Identifier.Text,
                            invocationSyntax.ArgumentList.Arguments
                        );
                    }

                    if (!ValidateTail())
                    {
                        throw new InvalidOperationException("Invalid tail.");
                    }

                    invocationSyntaxis.Clear();
                }
            }

            protected abstract bool ValidateHead(
                string name,
                SeparatedSyntaxList<ArgumentSyntax> arguments
            );
            protected abstract void ReadTail(
                string name,
                SeparatedSyntaxList<ArgumentSyntax> arguments
            );

            protected virtual bool ValidateTail()
            {
                return true;
            }
        }
    }
}
