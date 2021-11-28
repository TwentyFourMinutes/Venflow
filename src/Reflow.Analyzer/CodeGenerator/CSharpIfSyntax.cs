using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpIfSyntax
    {
        private IfStatementSyntax _ifSyntax;

        public CSharpIfSyntax(IfStatementSyntax ifSyntax)
        {
            _ifSyntax = ifSyntax;
        }

        public CSharpIfSyntax(ExpressionSyntax condition, IEnumerable<StatementSyntax> then)
        {
            _ifSyntax = IfStatement(condition, Block(then));
        }

        public static implicit operator IfStatementSyntax(CSharpIfSyntax syntax)
        {
            return syntax._ifSyntax;
        }

        public CSharpIfSyntax Else(params StatementSyntax[] statements)
        {
            return Else((IEnumerable<StatementSyntax>)statements);
        }

        public CSharpIfSyntax Else(IEnumerable<StatementSyntax> statements)
        {
            _ifSyntax = _ifSyntax.WithElse(ElseClause(Block(statements)));

            return this;
        }
    }
}
