using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Reflow.Analyzer.Shared;

namespace Reflow.Analyzer.Diagnostic
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UninitializedTableWarningSuppressor : DiagnosticSuppressor
    {
        private readonly static Func<
            Microsoft.CodeAnalysis.Diagnostic,
            IReadOnlyList<object>
        > _getDiagnosticParameters;

        static UninitializedTableWarningSuppressor()
        {
            var diagnosticParameter = Expression.Parameter(
                typeof(Microsoft.CodeAnalysis.Diagnostic)
            );

            _getDiagnosticParameters = Expression
                .Lambda<Func<Microsoft.CodeAnalysis.Diagnostic, IReadOnlyList<object>>>(
                    Expression.MakeMemberAccess(
                        diagnosticParameter,
                        diagnosticParameter.Type.GetProperty(
                            "Arguments",
                            BindingFlags.Instance | BindingFlags.NonPublic
                        )
                    ),
                    diagnosticParameter
                )
                .Compile();
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            new[]
            {
                new SuppressionDescriptor(
                    "RFSUPP1001",
                    "CS8618",
                    "This property is guaranteed to be initialized by a base constructor."
                )
            }.ToImmutableArray();

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            SemanticModel? semanticModel = null;

            for (
                var diagnosticIndex = 0;
                diagnosticIndex < context.ReportedDiagnostics.Length;
                diagnosticIndex++
            )
            {
                var diagnostic = context.ReportedDiagnostics[diagnosticIndex];

                if (string.Equals(diagnostic.Id, "CS8618", StringComparison.Ordinal))
                {
                    var arguments = _getDiagnosticParameters.Invoke(diagnostic);

                    if (
                        arguments[0].ToString() is not "property"
                        || diagnostic.Location.SourceTree!
                            .GetRoot(context.CancellationToken)
                            .FindNode(diagnostic.Location.SourceSpan)
                            is not ConstructorDeclarationSyntax constructorSyntax
                    )
                    {
                        continue;
                    }

                    semanticModel ??= context.GetSemanticModel(diagnostic.Location.SourceTree);

                    var classType = (INamedTypeSymbol)(
                        semanticModel.GetDeclaredSymbol(
                            constructorSyntax.FirstAncestorOrSelf<ClassDeclarationSyntax>()!,
                            context.CancellationToken
                        )!
                    );

                    var isDatabase = false;

                    while (classType.BaseType is not null)
                    {
                        if (
                            classType.BaseType.OriginalDefinition.GetFullName()
                            is "Reflow.Database`1"
                        )
                        {
                            isDatabase = true;
                            break;
                        }

                        classType = classType.BaseType;
                    }

                    if (
                        !isDatabase
                        || classType.GetMembers((string)arguments[1]).Single()
                            is not IPropertySymbol propertySymbol
                        || propertySymbol.Type.OriginalDefinition.GetFullName()
                            is not "Reflow.Table`1"
                    )
                    {
                        continue;
                    }

                    context.ReportSuppression(
                        Suppression.Create(SupportedSuppressions[0], diagnostic)
                    );
                }
            }
        }
    }
}
