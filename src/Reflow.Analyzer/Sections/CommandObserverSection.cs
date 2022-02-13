using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Operations;
using Reflow.Analyzer.Shared;

namespace Reflow.Analyzer.Sections
{
    internal class CommandObserverSection
        : GeneratorSection<
              EntityConfigurationSection,
              CommandObserverSection.SyntaxReceiver,
              Dictionary<ITypeSymbol, List<Command>>
          >
    {
        protected override Dictionary<ITypeSymbol, List<Command>> Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            EntityConfigurationSection previous
        )
        {
            var databaseCommands = new Dictionary<ITypeSymbol, List<Command>>(
                SymbolEqualityComparer.Default
            );

            for (
                var operationSyntaxIndex = 0;
                operationSyntaxIndex < syntaxReceiver.Candidates.Count;
                operationSyntaxIndex++
            )
            {
                var operationSyntax = syntaxReceiver.Candidates[operationSyntaxIndex];

                var memberAccessSyntax = (MemberAccessExpressionSyntax)operationSyntax.Expression;

                var semanticModel = context.Compilation.GetSemanticModel(
                    operationSyntax.SyntaxTree
                );

                var databaseType = semanticModel.GetTypeInfo(
                    ((MemberAccessExpressionSyntax)memberAccessSyntax.Expression).Expression
                ).Type!;

                if (!databaseCommands.TryGetValue(databaseType, out var commands))
                {
                    databaseCommands.Add(databaseType, commands = new List<Command>());
                }

                var invocationSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(
                    operationSyntax.Expression
                ).Symbol!;

                var entitySymbol = (
                    (INamedTypeSymbol)semanticModel.GetTypeInfo(
                        (MemberAccessExpressionSyntax)memberAccessSyntax.Expression
                    ).Type!
                ).TypeArguments[0];

                var parameterType = invocationSymbol.Parameters[0].Type;

                var operationType =
                    parameterType.GetFullName() == "System.Collections.IList`1"
                        ? OperationType.Many
                        : OperationType.Single;

                var commandType = memberAccessSyntax.Name.Identifier.Text switch
                {
                    "InsertAsync" => Command.CommandType.Insert,
                    _ => throw new InvalidOperationException(),
                };

                commands.Add(new Command(operationType, commandType, entitySymbol));
            }

            return databaseCommands;
        }

        internal class SyntaxReceiver : ISyntaxContextReceiver
        {
            private static readonly HashSet<string> _validInvocationNames =
                new() { "UpdateAsync", "InsertAsync", "DeleteAsync" };

            internal List<InvocationExpressionSyntax> Candidates { get; }

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
