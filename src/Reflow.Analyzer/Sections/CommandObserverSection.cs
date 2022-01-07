using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Sections
{
    internal class CommandObserverSection
        : GeneratorSection<
              EntityConfigurationSection,
              CommandObserverSection.SyntaxReceiver,
              NoData
          >
    {
        protected override NoData Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            EntityConfigurationSection previous
        )
        {
            return default;
        }

        internal class SyntaxReceiver : ISyntaxContextReceiver
        {
            private static readonly HashSet<string> _validInvocationNames =
                new() { "UpdateAsync", "InsertAsync", "DeleteAsync" };

            internal HashSet<InvocationExpressionSyntax> Candidates { get; }

            internal SyntaxReceiver()
            {
                Candidates = new();
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is not InvocationExpressionSyntax invocationSyntax)
                    return;

                var memberAccessSyntax = invocationSyntax
                    .ChildNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .FirstOrDefault();

                if (
                    memberAccessSyntax is null
                    || !_validInvocationNames.Contains(memberAccessSyntax.Name.Identifier.Text)
                )
                {
                    return;
                }

                memberAccessSyntax = memberAccessSyntax
                    .ChildNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .FirstOrDefault();

                if (memberAccessSyntax is null)
                {
                    return;
                }

                var type = context.SemanticModel.GetTypeInfo(memberAccessSyntax).Type!;

                if (!type.IsReflowSymbol())
                    return;

                Candidates.Add(invocationSyntax);
            }
        }
    }
}
