using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Internal
{
    public class CSharpAttributeSyntax
    {
        private AttributeSyntax _attributeSyntax;

        public CSharpAttributeSyntax(string name)
        {
            _attributeSyntax = Attribute(IdentifierName(name));
        }

        public static implicit operator AttributeSyntax(CSharpAttributeSyntax syntax)
        {
            return syntax._attributeSyntax;
        }

        public CSharpAttributeSyntax WithArguments(params ExpressionSyntax[] parameters)
        {
            _attributeSyntax = _attributeSyntax.WithArgumentList(
                AttributeArgumentList(SeparatedList(parameters.Select(x => AttributeArgument(x))))
            );

            return this;
        }
    }
}
