using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpLocalSyntax
    {
        private LocalDeclarationStatementSyntax _localSyntax;

        public CSharpLocalSyntax(string name, TypeSyntax type, CSharpModifiers modifiers)
        {
            _localSyntax = LocalDeclarationStatement(
                VariableDeclaration(type, SingletonSeparatedList(VariableDeclarator(name)))
            );

            if (modifiers != CSharpModifiers.None)
            {
                _localSyntax = _localSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator StatementSyntax(CSharpLocalSyntax syntax)
        {
            return syntax._localSyntax;
        }

        public CSharpLocalSyntax WithInitializer(ExpressionSyntax expression)
        {
            _localSyntax = _localSyntax.WithDeclaration(
                _localSyntax.Declaration.WithVariables(
                    SingletonSeparatedList(
                        _localSyntax.Declaration.Variables[0].WithInitializer(
                            EqualsValueClause(expression)
                        )
                    )
                )
            );

            return this;
        }
    }
}
