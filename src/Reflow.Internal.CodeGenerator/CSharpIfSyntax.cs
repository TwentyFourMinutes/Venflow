using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Internal
{
    public class CSharpIfSyntax
    {
        private readonly IfStatementSyntax _ifSyntax;

        public CSharpIfSyntax(ExpressionSyntax condition, StatementSyntax[] then)
        {
            _ifSyntax = IfStatement(condition, Block(then));
        }

        public static implicit operator IfStatementSyntax(CSharpIfSyntax syntax)
        {
            return syntax._ifSyntax;
        }
    }
}
