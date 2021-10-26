using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Internal
{
    public class CSharpClassSyntax
    {
        private ClassDeclarationSyntax _classSyntax;

        public CSharpClassSyntax(string name, CSharpModifiers modifiers)
        {
            _classSyntax = ClassDeclaration(name);

            if (modifiers != CSharpModifiers.None)
            {
                _classSyntax = _classSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator SyntaxNode(CSharpClassSyntax syntax)
        {
            return syntax._classSyntax;
        }

        public CSharpClassSyntax WithMembers(params SyntaxNode[] members)
        {
            return WithMembers((IEnumerable<SyntaxNode>)members);
        }

        public CSharpClassSyntax WithMembers(IEnumerable<SyntaxNode> members)
        {
            _classSyntax = _classSyntax.WithMembers(List(members));
            return this;
        }

        public CSharpClassSyntax WithBase(TypeSyntax type)
        {
            _classSyntax = _classSyntax.WithBaseList(
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(type)))
            );

            return this;
        }

        public CSharpClassSyntax WithAttributes(params CSharpAttributeSyntax[] attributes)
        {
            _classSyntax = _classSyntax.WithAttributeLists(
                SingletonList(
                    AttributeList(SeparatedList(attributes.Select(x => (AttributeSyntax)x)))
                )
            );

            return this;
        }
    }
}
