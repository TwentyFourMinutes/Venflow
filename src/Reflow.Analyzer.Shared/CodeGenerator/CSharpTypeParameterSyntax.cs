using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpTypeParameterSyntax
    {
        private readonly TypeParameterSyntax _parameterSyntax;

        public CSharpTypeParameterSyntax(string name)
        {
            _parameterSyntax = TypeParameter(Identifier(name));
        }

        public static implicit operator TypeParameterSyntax(CSharpTypeParameterSyntax syntax)
        {
            return syntax._parameterSyntax;
        }
    }
}
