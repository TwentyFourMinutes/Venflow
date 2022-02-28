using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpPropertySyntax
    {
        private PropertyDeclarationSyntax _propertySyntax;

        public CSharpPropertySyntax(string name, TypeSyntax type, CSharpModifiers modifiers)
        {
            _propertySyntax = PropertyDeclaration(type, name);

            if (modifiers != CSharpModifiers.None)
            {
                _propertySyntax = _propertySyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }

            WithAttributes(
                CSharpCodeGenerator
                    .Attribute(CSharpCodeGenerator.Type<EditorBrowsableAttribute>())
                    .WithArguments(CSharpCodeGenerator.EnumMember(EditorBrowsableState.Never))
            );
        }

        public static implicit operator MemberDeclarationSyntax(CSharpPropertySyntax syntax)
        {
            return syntax._propertySyntax;
        }

        public CSharpPropertySyntax WithInitializer(ExpressionSyntax expression)
        {
            _propertySyntax = _propertySyntax.WithInitializer(EqualsValueClause(expression));

            return this;
        }

        public CSharpPropertySyntax WithGetAccessor(params StatementSyntax[] statements)
        {
            _propertySyntax = _propertySyntax.AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(Block(List(statements)))
            );

            return this;
        }

        public CSharpPropertySyntax WithSetAccessor(params StatementSyntax[] statements)
        {
            _propertySyntax = _propertySyntax.AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(Block(List(statements)))
            );

            return this;
        }

        public CSharpPropertySyntax WithAttributes(params CSharpAttributeSyntax[] attributes)
        {
            _propertySyntax = _propertySyntax.WithAttributeLists(
                SingletonList(
                    AttributeList(SeparatedList(attributes.Select(x => (AttributeSyntax)x)))
                )
            );

            return this;
        }
    }
}
