using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpStructSyntax
    {
        private StructDeclarationSyntax _structSyntax;

        public CSharpStructSyntax(string name, CSharpModifiers modifiers)
        {
            _structSyntax = StructDeclaration(name);

            if (modifiers != CSharpModifiers.None)
            {
                _structSyntax = _structSyntax.WithModifiers(modifiers.GetSyntaxTokens());
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

        public static implicit operator SyntaxNode(CSharpStructSyntax syntax)
        {
            return syntax._structSyntax;
        }

        public CSharpStructSyntax WithMembers(params SyntaxNode[] members)
        {
            return WithMembers((IEnumerable<SyntaxNode>)members);
        }

        public CSharpStructSyntax WithMembers(IEnumerable<SyntaxNode> members)
        {
            _structSyntax = _structSyntax.WithMembers(List(members));
            return this;
        }

        public CSharpStructSyntax WithOptionalMember(
            bool condition,
            Func<MemberDeclarationSyntax> memberFunc
        )
        {
            if (condition)
            {
                _structSyntax = _structSyntax.AddMembers(memberFunc.Invoke());
            }
            return this;
        }

        public CSharpStructSyntax WithBase(TypeSyntax type)
        {
            _structSyntax = _structSyntax.WithBaseList(
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(type)))
            );

            return this;
        }

        public CSharpStructSyntax WithAttributes(params CSharpAttributeSyntax[] attributes)
        {
            return WithAttributes((IEnumerable<CSharpAttributeSyntax>)attributes);
        }

        public CSharpStructSyntax WithAttributes(IEnumerable<CSharpAttributeSyntax> attributes)
        {
            _structSyntax = _structSyntax.WithAttributeLists(
                SingletonList(
                    AttributeList(SeparatedList(attributes.Select(x => (AttributeSyntax)x)))
                )
            );

            return this;
        }

        public CSharpStructSyntax WithTypeParameters(params CSharpTypeParameterSyntax[] parameters)
        {
            _structSyntax = _structSyntax.WithTypeParameterList(
                TypeParameterList(SeparatedList(parameters.Select(x => (TypeParameterSyntax)x)))
            );

            return this;
        }

        public CSharpStructSyntax WithBaseTypes(params TypeSyntax[] parameters)
        {
            _structSyntax = _structSyntax.WithBaseList(
                BaseList(SeparatedList(parameters.Select(x => (BaseTypeSyntax)SimpleBaseType(x))))
            );

            return this;
        }
    }
}
