using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpParameterSyntax
    {
        private ParameterSyntax _parameterSyntax;

        public CSharpParameterSyntax(string name, TypeSyntax type, CSharpModifiers modifiers)
        {
            _parameterSyntax = Parameter(Identifier(name)).WithType(type);

            if (modifiers != CSharpModifiers.None)
            {
                _parameterSyntax = _parameterSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator ParameterSyntax(CSharpParameterSyntax syntax)
        {
            return syntax._parameterSyntax;
        }

        public CSharpParameterSyntax WithDefault(SyntaxKind syntax)
        {
            _parameterSyntax = _parameterSyntax.WithDefault(
                EqualsValueClause(LiteralExpression(syntax))
            );

            return this;
        }
    }
}
