using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpConstructorSyntax
    {
        private ConstructorDeclarationSyntax _constructorSyntax;

        public CSharpConstructorSyntax(string name, CSharpModifiers modifiers)
        {
            _constructorSyntax = ConstructorDeclaration(name);

            if (modifiers != CSharpModifiers.None)
            {
                _constructorSyntax = _constructorSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }

            WithAttributes(
                CSharpCodeGenerator
                    .Attribute(CSharpCodeGenerator.Type<EditorBrowsableAttribute>())
                    .WithArguments(CSharpCodeGenerator.EnumMember(EditorBrowsableState.Never))
            );
        }

        public static implicit operator ConstructorDeclarationSyntax(CSharpConstructorSyntax syntax)
        {
            return syntax._constructorSyntax;
        }

        public CSharpConstructorSyntax WithParameters(params CSharpParameterSyntax[] parameters)
        {
            _constructorSyntax = _constructorSyntax.WithParameterList(
                ParameterList(SeparatedList(parameters.Select(x => (ParameterSyntax)x)))
            );

            return this;
        }

        public CSharpConstructorSyntax WithStatements(params StatementSyntax[] statements)
        {
            return WithStatements((IEnumerable<StatementSyntax>)statements);
        }

        public CSharpConstructorSyntax WithStatements(IEnumerable<StatementSyntax> statements)
        {
            _constructorSyntax = _constructorSyntax.WithBody(Block(statements));

            return this;
        }

        public CSharpConstructorSyntax WithAttributes(params CSharpAttributeSyntax[] attributes)
        {
            _constructorSyntax = _constructorSyntax.WithAttributeLists(
                SingletonList(
                    AttributeList(SeparatedList(attributes.Select(x => (AttributeSyntax)x)))
                )
            );

            return this;
        }
    }
}
