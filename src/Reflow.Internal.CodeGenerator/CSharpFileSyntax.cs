using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Reflow.Internal.CSharpCodeGenerator;

namespace Reflow.Internal
{
    public class CSharpFileSyntax
    {
        private NamespaceDeclarationSyntax _namespaceSyntax;

        public CSharpFileSyntax(string namespaceName)
        {
            _namespaceSyntax = NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceName));
        }

        public CSharpFileSyntax WithMembers(params SyntaxNode[] members)
        {
            return WithMembers((IEnumerable<SyntaxNode>)members);
        }

        public CSharpFileSyntax WithMembers(IEnumerable<SyntaxNode> members)
        {
            _namespaceSyntax = _namespaceSyntax.WithMembers(List(members));

            return this;
        }

        public CSharpFileSyntax WithUsings(params string[] members)
        {
            return WithUsings((IEnumerable<string>)members);
        }

        public CSharpFileSyntax WithUsings(IEnumerable<string> members)
        {
            _namespaceSyntax = _namespaceSyntax.WithUsings(
                List(members.Select(x => UsingDirective(Name(x))))
            );

            return this;
        }

        public SourceText GetText()
        {
            return CompilationUnit()
                .WithMembers(SingletonList<MemberDeclarationSyntax>(_namespaceSyntax))
                .NormalizeWhitespace()
                .GetText(Encoding.UTF8);
        }
    }
}
