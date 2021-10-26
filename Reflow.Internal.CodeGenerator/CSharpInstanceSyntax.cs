using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Internal
{
    public class CSharpInstanceSyntax
    {
        private ObjectCreationExpressionSyntax _objectCreationSyntax;

        public CSharpInstanceSyntax(TypeSyntax type)
        {
            _objectCreationSyntax = ObjectCreationExpression(type);
        }

        public static implicit operator ExpressionSyntax(CSharpInstanceSyntax syntax)
        {
            return syntax._objectCreationSyntax;
        }

        public CSharpInstanceSyntax WithInitializer(InitializerExpressionSyntax expression)
        {
            _objectCreationSyntax = _objectCreationSyntax.WithInitializer(expression);

            return this;
        }
        public CSharpInstanceSyntax WithArguments(params ExpressionSyntax[] members)
        {
            return WithArguments((IEnumerable<ExpressionSyntax>)members);
        }
        public CSharpInstanceSyntax WithArguments(IEnumerable<ExpressionSyntax> members)
        {
            _objectCreationSyntax = _objectCreationSyntax.WithArgumentList(
                ArgumentList(SeparatedList(members.Select(x => Argument(x))))
            );

            return this;
        }
    }
}
