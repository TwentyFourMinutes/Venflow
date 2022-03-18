using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpLambdaSyntax
    {
        private ParenthesizedLambdaExpressionSyntax _lambdaSyntax;

        public CSharpLambdaSyntax(string[] parameters)
        {
            _lambdaSyntax = ParenthesizedLambdaExpression()
                .WithParameterList(
                    ParameterList(SeparatedList(parameters.Select(x => Parameter(Identifier(x)))))
                );
        }

        public static implicit operator ExpressionSyntax(CSharpLambdaSyntax syntax)
        {
            return syntax._lambdaSyntax;
        }

        public CSharpLambdaSyntax WithStatements(params StatementSyntax[] statements)
        {
            return WithStatements((IEnumerable<StatementSyntax>)statements);
        }

        public CSharpLambdaSyntax WithStatements(IEnumerable<StatementSyntax> statements)
        {
            _lambdaSyntax = _lambdaSyntax.WithBody(Block(statements));

            return this;
        }
    }
}
