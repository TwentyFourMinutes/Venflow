using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpAttributeSyntax
    {
        private AttributeSyntax _attributeSyntax;

        public CSharpAttributeSyntax(TypeSyntax type)
        {
            _attributeSyntax = Attribute(IdentifierName(type.ToString()));
        }

        public static implicit operator AttributeSyntax(CSharpAttributeSyntax syntax)
        {
            return syntax._attributeSyntax;
        }

        public CSharpAttributeSyntax WithArguments(params ExpressionSyntax[] parameters)
        {
            _attributeSyntax = _attributeSyntax.WithArgumentList(
                AttributeArgumentList(SeparatedList(parameters.Select(AttributeArgument)))
            );

            return this;
        }
    }
}
