using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Shared;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer
{
    [Generator(LanguageNames.CSharp)]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var keyDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider<(INamedTypeSymbol StructSymbol, INamedTypeSymbol? UnderlyingSymbol)>(
                    static (node, _) =>
                        node is StructDeclarationSyntax structSyntax
                        && structSyntax.AttributeLists.Count > 0
                        && structSyntax.Modifiers.Any(
                            static x => x.IsKind(SyntaxKind.PartialKeyword)
                        ),
                    static (ctx, ct) =>
                    {
                        var structSyntax = (StructDeclarationSyntax)ctx.Node;

                        INamedTypeSymbol? underlyingSymbol = null;

                        for (
                            var attributeIndex = 0;
                            attributeIndex < structSyntax.AttributeLists.Count;
                            attributeIndex++
                        )
                        {
                            var attribtueList = structSyntax.AttributeLists[attributeIndex];

                            foreach (AttributeSyntax attributeSyntax in attribtueList.ChildNodes())
                            {
                                if (
                                    (
                                        attributeSyntax.Name is GenericNameSyntax genericName
                                        && genericName.Identifier.Text.Contains("GeneratedKey")
                                    )
                                    || (
                                        attributeSyntax.Name is IdentifierNameSyntax identiferName
                                        && identiferName.Identifier.Text.Contains("GeneratedKey")
                                    )
                                )
                                {
                                    var attribtueSymbol =
                                        (IMethodSymbol)ctx.SemanticModel.GetSymbolInfo(
                                            attributeSyntax,
                                            ct
                                        ).Symbol!;

                                    if (
                                        !(
                                            (INamedTypeSymbol)attribtueSymbol.ReceiverType!
                                        ).OriginalDefinition.IsReflowSymbol()
                                    )
                                    {
                                        continue;
                                    }

                                    var attributeName = attribtueSymbol.ReceiverType!.GetFullName();

                                    if (attributeName is "Reflow.GeneratedKey")
                                    {
                                        var typeOfSyntax =
                                            (TypeOfExpressionSyntax)attributeSyntax.ArgumentList!
                                                .ChildNodes()
                                                .Single();

                                        underlyingSymbol =
                                            (INamedTypeSymbol)ctx.SemanticModel.GetSymbolInfo(
                                                typeOfSyntax.Type,
                                                ct
                                            ).Symbol!;
                                    }
                                    else if (attributeName is "Reflow.GeneratedKey`1")
                                    {
                                        underlyingSymbol = (INamedTypeSymbol)(
                                            (INamedTypeSymbol)attribtueSymbol.ReceiverType!
                                        ).TypeArguments[0];
                                    }
                                }
                            }
                        }

                        if (underlyingSymbol is null)
                            return (null!, null);
                        else
                            return (
                                ctx.SemanticModel.GetDeclaredSymbol(structSyntax)!,
                                underlyingSymbol
                            );
                    }
                )
                .Where(static x => x.UnderlyingSymbol is not null);

            IncrementalValueProvider<(Compilation Compilation, ImmutableArray<(INamedTypeSymbol StructSymbol, INamedTypeSymbol? UnderlyingSymbol)> KeyDeclarations)> compilationKeyDeclerations =
                context.CompilationProvider.Combine(keyDeclarations.Collect());

            context.RegisterSourceOutput(
                compilationKeyDeclerations,
                static (spc, combined) =>
                {
                    if (combined.KeyDeclarations.IsDefaultOrEmpty)
                        return;

                    Options.Init(combined.Compilation);
                    Options.HideFromEditor = false;

                    var namespaceDeclarations = new List<NamespaceDeclarationSyntax>();

                    foreach (
                        var (structSymbol, underlyingSymbol) in combined.KeyDeclarations.Distinct()
                    )
                    {
                        var namespaceName = structSymbol.GetNamespace();
                        var fullName = namespaceName + "." + structSymbol.Name;

                        namespaceDeclarations.Add(
                            Namespace(namespaceName)
                                .WithMembers(
                                    Struct(structSymbol.Name, CSharpModifiers.Partial)
                                        .WithTypeParameters(TypeParameter("T"))
                                        .WithMembers(
                                            Field(
                                                "_value",
                                                Type(underlyingSymbol!),
                                                CSharpModifiers.Private | CSharpModifiers.ReadOnly
                                            ),
                                            Constructor(structSymbol.Name, CSharpModifiers.Public)
                                                .WithParameters(
                                                    Parameter("value", Type(underlyingSymbol!))
                                                )
                                                .WithStatements(
                                                    AssignMember(
                                                        This(),
                                                        "_value",
                                                        Variable("value")
                                                    )
                                                ),
                                            ImplicitOperator(
                                                    Type(underlyingSymbol!),
                                                    CSharpModifiers.Public | CSharpModifiers.Static
                                                )
                                                .WithParameters(
                                                    Parameter(
                                                        "key",
                                                        GenericType(fullName, Generic("T"))
                                                    )
                                                )
                                                .WithStatements(
                                                    Return(AccessMember(Variable("key"), "_value"))
                                                ),
                                            ImplicitOperator(
                                                    GenericType(fullName, Generic("T")),
                                                    CSharpModifiers.Public | CSharpModifiers.Static
                                                )
                                                .WithParameters(
                                                    Parameter("value", Type(underlyingSymbol!))
                                                )
                                                .WithStatements(
                                                    Return(
                                                        Instance(
                                                                GenericType(fullName, Generic("T"))
                                                            )
                                                            .WithArguments(Variable("value"))
                                                    )
                                                )
                                        )
                                )
                        );
                    }

                    spc.AddSource(
                        "Keys.g.cs",
                        Compilation(namespaceDeclarations)
                            .NormalizeWhitespace()
                            .GetText(Encoding.UTF8)
                    );
                }
            );
        }
    }
}
