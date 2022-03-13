using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpInterfaceSyntax
    {
        private InterfaceDeclarationSyntax _interfaceSyntax;

        public CSharpInterfaceSyntax(string name, CSharpModifiers modifiers)
        {
            _interfaceSyntax = InterfaceDeclaration(name);

            if (modifiers != CSharpModifiers.None)
            {
                _interfaceSyntax = _interfaceSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }

            if (CSharpCodeGenerator.Options.EmitSkipLocalsInit)
            {
                WithAttributes(
                    CSharpCodeGenerator.Attribute(
                        CSharpCodeGenerator.Type(
                            "System.Runtime.CompilerServices.SkipLocalsInitAttribute"
                        )
                    )
                );
            }
        }

        public static implicit operator SyntaxNode(CSharpInterfaceSyntax syntax)
        {
            return syntax._interfaceSyntax;
        }

        public CSharpInterfaceSyntax WithMembers(params SyntaxNode[] members)
        {
            return WithMembers((IEnumerable<SyntaxNode>)members);
        }

        public CSharpInterfaceSyntax WithMembers(IEnumerable<SyntaxNode> members)
        {
            _interfaceSyntax = _interfaceSyntax.WithMembers(List(members));
            return this;
        }

        public CSharpInterfaceSyntax WithBase(TypeSyntax type)
        {
            _interfaceSyntax = _interfaceSyntax.WithBaseList(
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(type)))
            );

            return this;
        }

        public CSharpInterfaceSyntax WithAttributes(params CSharpAttributeSyntax[] attributes)
        {
            _interfaceSyntax = _interfaceSyntax.WithAttributeLists(
                SingletonList(
                    AttributeList(SeparatedList(attributes.Select(x => (AttributeSyntax)x)))
                )
            );

            return this;
        }
    }
}
