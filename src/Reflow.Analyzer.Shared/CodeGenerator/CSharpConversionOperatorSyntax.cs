using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpConversionOperatorSyntax
    {
        private ConversionOperatorDeclarationSyntax _operatorSyntax;

        public CSharpConversionOperatorSyntax(
            SyntaxKind syntaxKind,
            TypeSyntax toType,
            CSharpModifiers modifiers
        )
        {
            _operatorSyntax = ConversionOperatorDeclaration(Token(syntaxKind), toType);

            if (modifiers != CSharpModifiers.None)
            {
                _operatorSyntax = _operatorSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }

            if (CSharpCodeGenerator.Options.HideFromEditor)
            {
                WithAttributes(
                    CSharpCodeGenerator
                        .Attribute(CSharpCodeGenerator.Type<EditorBrowsableAttribute>())
                        .WithArguments(CSharpCodeGenerator.EnumMember(EditorBrowsableState.Never))
                );
            }
        }

        public static implicit operator ConversionOperatorDeclarationSyntax(
            CSharpConversionOperatorSyntax syntax
        )
        {
            return syntax._operatorSyntax;
        }

        public CSharpConversionOperatorSyntax WithParameters(
            params CSharpParameterSyntax[] parameters
        )
        {
            _operatorSyntax = _operatorSyntax.WithParameterList(
                ParameterList(SeparatedList(parameters.Select(x => (ParameterSyntax)x)))
            );

            return this;
        }

        public CSharpConversionOperatorSyntax WithStatements(params StatementSyntax[] statements)
        {
            return WithStatements((IEnumerable<StatementSyntax>)statements);
        }

        public CSharpConversionOperatorSyntax WithStatements(
            IEnumerable<StatementSyntax> statements
        )
        {
            _operatorSyntax = _operatorSyntax.WithBody(Block(statements));

            return this;
        }

        public CSharpConversionOperatorSyntax WithAttributes(
            params CSharpAttributeSyntax[] attributes
        )
        {
            _operatorSyntax = _operatorSyntax.WithAttributeLists(
                SingletonList(
                    AttributeList(SeparatedList(attributes.Select(x => (AttributeSyntax)x)))
                )
            );

            return this;
        }
    }
}
