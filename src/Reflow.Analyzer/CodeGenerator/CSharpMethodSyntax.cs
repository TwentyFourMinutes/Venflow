using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpMethodSyntax
    {
        private MethodDeclarationSyntax _methodSyntax;

        public CSharpMethodSyntax(string name, TypeSyntax returnType, CSharpModifiers modifiers)
        {
            _methodSyntax = MethodDeclaration(returnType, name);

            if (modifiers != CSharpModifiers.None)
            {
                _methodSyntax = _methodSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator MethodDeclarationSyntax(CSharpMethodSyntax syntax)
        {
            return syntax._methodSyntax;
        }

        public CSharpMethodSyntax WithParameters(params CSharpParameterSyntax[] parameters)
        {
            _methodSyntax = _methodSyntax.WithParameterList(
                ParameterList(SeparatedList(parameters.Select(x => (ParameterSyntax)x)))
            );

            return this;
        }

        public CSharpMethodSyntax WithStatements(params StatementSyntax[] statements)
        {
            return WithStatements((IEnumerable<StatementSyntax>)statements);
        }

        public CSharpMethodSyntax WithStatements(IEnumerable<StatementSyntax> statements)
        {
            _methodSyntax = _methodSyntax.WithBody(Block(statements));

            return this;
        }
    }
}
