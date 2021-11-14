using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpFieldSyntax
    {
        private FieldDeclarationSyntax _fieldSyntax;

        public CSharpFieldSyntax(string name, TypeSyntax type, CSharpModifiers modifiers)
        {
            _fieldSyntax = FieldDeclaration(
                VariableDeclaration(type, SingletonSeparatedList(VariableDeclarator(name)))
            );

            if (modifiers != CSharpModifiers.None)
            {
                _fieldSyntax = _fieldSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator MemberDeclarationSyntax(CSharpFieldSyntax syntax)
        {
            return syntax._fieldSyntax;
        }

        public CSharpFieldSyntax WithInitializer(ExpressionSyntax expression)
        {
            _fieldSyntax = _fieldSyntax.WithDeclaration(
                _fieldSyntax.Declaration.WithVariables(
                    SingletonSeparatedList(
                        _fieldSyntax.Declaration.Variables[0].WithInitializer(
                            EqualsValueClause(expression)
                        )
                    )
                )
            );

            return this;
        }
    }
}
