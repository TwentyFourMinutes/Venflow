using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpNamespaceSyntax
    {
        private NamespaceDeclarationSyntax _namespaceSyntax;

        public CSharpNamespaceSyntax(string name)
        {
            _namespaceSyntax = NamespaceDeclaration(IdentifierName(name));
        }

        public static implicit operator NamespaceDeclarationSyntax(CSharpNamespaceSyntax syntax)
        {
            return syntax._namespaceSyntax;
        }

        public CSharpNamespaceSyntax WithMembers(params SyntaxNode[] members)
        {
            return WithMembers((IEnumerable<SyntaxNode>)members);
        }

        public CSharpNamespaceSyntax WithMembers(IEnumerable<SyntaxNode> members)
        {
            _namespaceSyntax = _namespaceSyntax.WithMembers(List(members));

            return this;
        }
    }
}
