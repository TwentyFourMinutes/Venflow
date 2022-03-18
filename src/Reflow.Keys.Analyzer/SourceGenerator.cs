using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
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

                    var emitSystemTextJson = combined.Compilation.HasAssemblyReference(
                        "System.Text.Json",
                        KnownPublicKeys.SystemTextJson
                    );

                    Options.Init(combined.Compilation);

                    var namespaceDeclarations = new List<NamespaceDeclarationSyntax>();

                    if (emitSystemTextJson)
                    {
                        namespaceDeclarations.Add(
                            Namespace("Reflow")
                                .WithMembers(
                                    Class("JsonKeyConverterFactory", CSharpModifiers.Internal)
                                        .WithBase(
                                            Type(
                                                "System.Text.Json.Serialization.JsonConverterFactory"
                                            )
                                        )
                                        .WithMembers(
                                            Method(
                                                    "CanConvert",
                                                    Type<bool>(),
                                                    CSharpModifiers.Public
                                                        | CSharpModifiers.Override
                                                )
                                                .WithParameters(
                                                    Parameter("typeToConvert", Type<Type>())
                                                )
                                                .WithStatements(
                                                    Return(
                                                        Invoke(
                                                            Type(typeof(Enumerable)),
                                                            nameof(Enumerable.Any),
                                                            Invoke(
                                                                Variable("typeToConvert"),
                                                                nameof(System.Type.GetInterfaces)
                                                            ),
                                                            Lambda("x")
                                                                .WithStatements(
                                                                    Return(
                                                                        Equal(
                                                                            Variable("x"),
                                                                            TypeOf(
                                                                                Type("Reflow.IKey")
                                                                            )
                                                                        )
                                                                    )
                                                                )
                                                        )
                                                    )
                                                ),
                                            Method(
                                                    "CreateConverter",
                                                    NullableType(
                                                        "System.Text.Json.Serialization.JsonConverter"
                                                    ),
                                                    CSharpModifiers.Public
                                                        | CSharpModifiers.Override
                                                )
                                                .WithParameters(
                                                    Parameter("typeToConvert", Type<Type>()),
                                                    Parameter(
                                                        "options",
                                                        Type(
                                                            "System.Text.Json.JsonSerializerOptions"
                                                        )
                                                    )
                                                )
                                                .WithStatements(
                                                    If(
                                                        Not(
                                                            Invoke(
                                                                This(),
                                                                "CanConvert",
                                                                Variable("typeToConvert")
                                                            )
                                                        ),
                                                        Return(Null())
                                                    ),
                                                    Local("converterType", Var())
                                                        .WithInitializer(
                                                            Invoke(
                                                                Invoke(
                                                                    Variable("typeToConvert"),
                                                                    nameof(
                                                                        System.Type.GetNestedType
                                                                    ),
                                                                    Constant("JsonConverter"),
                                                                    EnumMember(
                                                                        BindingFlags.NonPublic
                                                                    )
                                                                ),
                                                                nameof(System.Type.MakeGenericType),
                                                                AccessElement(
                                                                    AccessMember(
                                                                        Variable("typeToConvert"),
                                                                        nameof(
                                                                            System.Type.GenericTypeArguments
                                                                        )
                                                                    ),
                                                                    Constant(0)
                                                                )
                                                            )
                                                        ),
                                                    Return(
                                                        Cast(
                                                            Type(
                                                                "System.Text.Json.Serialization.JsonConverter"
                                                            ),
                                                            Invoke(
                                                                Type(typeof(Activator)),
                                                                nameof(Activator.CreateInstance),
                                                                Variable("converterType")
                                                            )
                                                        )
                                                    )
                                                )
                                        )
                                )
                        );
                    }

                    Options.HideFromEditor = false;

                    foreach (
                        var (structSymbol, underlyingSymbol) in combined.KeyDeclarations.Distinct()
                    )
                    {
                        var namespaceName = structSymbol.GetNamespace();
                        var fullName = namespaceName + "." + structSymbol.Name;

                        var attributesSyntax = new List<CSharpAttributeSyntax>
                        {
                            Attribute(Type<TypeConverterAttribute>())
                                .WithArguments(TypeOf(Type("Reflow.KeyConverterFactory")))
                        };

                        if (emitSystemTextJson)
                        {
                            attributesSyntax.Add(
                                Attribute(Type("System.Text.Json.Serialization.JsonConverter"))
                                    .WithArguments(TypeOf(Type("Reflow.JsonKeyConverterFactory")))
                            );
                        }

                        namespaceDeclarations.Add(
                            Namespace(namespaceName)
                                .WithMembers(
                                    Struct(structSymbol.Name, CSharpModifiers.Partial)
                                        .WithAttributes(attributesSyntax)
                                        .WithTypeParameters(TypeParameter("T"))
                                        .WithBaseTypes(Type("Reflow.IKey"))
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
                                                ),
                                            Class("KeyConverter", CSharpModifiers.Private)
                                                .WithBase(Type<TypeConverter>())
                                                .WithMembers(
                                                    Field(
                                                            "_valueTypeDescriptor",
                                                            Type<TypeConverter>(),
                                                            CSharpModifiers.Private
                                                                | CSharpModifiers.Static
                                                                | CSharpModifiers.ReadOnly
                                                        )
                                                        .WithInitializer(
                                                            Invoke(
                                                                Type<TypeDescriptor>(),
                                                                nameof(TypeDescriptor.GetConverter),
                                                                TypeOf(Type(underlyingSymbol!))
                                                            )
                                                        ),
                                                    Method(
                                                            "ConvertFrom",
                                                            NullableType<object>(),
                                                            CSharpModifiers.Public
                                                                | CSharpModifiers.Override
                                                        )
                                                        .WithParameters(
                                                            Parameter(
                                                                "context",
                                                                Type<ITypeDescriptorContext>()
                                                            ),
                                                            Parameter(
                                                                "culture",
                                                                Type<CultureInfo>()
                                                            ),
                                                            Parameter("value", Type<object>())
                                                        )
                                                        .WithStatements(
                                                            Local("result", Var())
                                                                .WithInitializer(
                                                                    Invoke(
                                                                        Variable(
                                                                            "_valueTypeDescriptor"
                                                                        ),
                                                                        nameof(
                                                                            TypeConverter.ConvertFrom
                                                                        ),
                                                                        Variable("context"),
                                                                        Variable("culture"),
                                                                        Variable("value")
                                                                    )
                                                                ),
                                                            Return(
                                                                Conditional(
                                                                    Equal(
                                                                        Variable("result"),
                                                                        Null()
                                                                    ),
                                                                    Null(),
                                                                    Cast(
                                                                        GenericName(
                                                                            fullName,
                                                                            Generic("T")
                                                                        ),
                                                                        Cast(
                                                                            Type(underlyingSymbol!),
                                                                            Variable("result")
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        ),
                                                    Method(
                                                            "ConvertTo",
                                                            NullableType<object>(),
                                                            CSharpModifiers.Public
                                                                | CSharpModifiers.Override
                                                        )
                                                        .WithParameters(
                                                            Parameter(
                                                                "context",
                                                                Type<ITypeDescriptorContext>()
                                                            ),
                                                            Parameter(
                                                                "culture",
                                                                Type<CultureInfo>()
                                                            ),
                                                            Parameter("value", Type<object>()),
                                                            Parameter(
                                                                "destinationType",
                                                                Type<Type>()
                                                            )
                                                        )
                                                        .WithStatements(
                                                            Return(
                                                                Invoke(
                                                                    Variable(
                                                                        "_valueTypeDescriptor"
                                                                    ),
                                                                    nameof(TypeConverter.ConvertTo),
                                                                    Variable("context"),
                                                                    Variable("culture"),
                                                                    AccessMember(
                                                                        Parenthesis(
                                                                            Cast(
                                                                                GenericName(
                                                                                    fullName,
                                                                                    Generic("T")
                                                                                ),
                                                                                Cast(
                                                                                    Type(
                                                                                        underlyingSymbol!
                                                                                    ),
                                                                                    Variable(
                                                                                        "value"
                                                                                    )
                                                                                )
                                                                            )
                                                                        ),
                                                                        "_value"
                                                                    ),
                                                                    Variable("destinationType")
                                                                )
                                                            )
                                                        )
                                                )
                                        )
                                        .WithOptionalMember(
                                            emitSystemTextJson,
                                            () =>
                                                (MemberDeclarationSyntax)Class(
                                                        "JsonConverter",
                                                        CSharpModifiers.Private
                                                    )
                                                    .WithBase(
                                                        GenericType(
                                                            "System.Text.Json.Serialization.JsonConverter",
                                                            GenericType(fullName, Generic("T"))
                                                        )
                                                    )
                                                    .WithMembers(
                                                        Method(
                                                                "Read",
                                                                GenericType(fullName, Generic("T")),
                                                                CSharpModifiers.Public
                                                                    | CSharpModifiers.Override
                                                            )
                                                            .WithParameters(
                                                                Parameter(
                                                                    "reader",
                                                                    Type(
                                                                        "System.Text.Json.Utf8JsonReader"
                                                                    ),
                                                                    CSharpModifiers.Ref
                                                                ),
                                                                Parameter(
                                                                    "typeToConvert",
                                                                    Type<Type>()
                                                                ),
                                                                Parameter(
                                                                    "options",
                                                                    Type(
                                                                        "System.Text.Json.JsonSerializerOptions"
                                                                    )
                                                                )
                                                            )
                                                            .WithStatements(
                                                                If(
                                                                    Equal(
                                                                        AccessMember(
                                                                            Variable("reader"),
                                                                            "TokenType"
                                                                        ),
                                                                        AccessMember(
                                                                            Type(
                                                                                "System.Text.Json.JsonTokenType"
                                                                            ),
                                                                            "Null"
                                                                        )
                                                                    ),
                                                                    Return(Default())
                                                                ),
                                                                Return(
                                                                    Cast(
                                                                        GenericType(
                                                                            fullName,
                                                                            Generic("T")
                                                                        ),
                                                                        Invoke(
                                                                            Type(
                                                                                "System.Text.Json.JsonSerializer"
                                                                            ),
                                                                            GenericName(
                                                                                "Deserialize",
                                                                                Type(
                                                                                    underlyingSymbol!
                                                                                )
                                                                            ),
                                                                            Ref(Variable("reader")),
                                                                            Variable("options")
                                                                        )
                                                                    )
                                                                )
                                                            ),
                                                        Method(
                                                                "Write",
                                                                Void(),
                                                                CSharpModifiers.Public
                                                                    | CSharpModifiers.Override
                                                            )
                                                            .WithParameters(
                                                                Parameter(
                                                                    "writer",
                                                                    Type(
                                                                        "System.Text.Json.Utf8JsonWriter"
                                                                    )
                                                                ),
                                                                Parameter(
                                                                    "value",
                                                                    GenericType(
                                                                        fullName,
                                                                        Generic("T")
                                                                    )
                                                                ),
                                                                Parameter(
                                                                    "options",
                                                                    Type(
                                                                        "System.Text.Json.JsonSerializerOptions"
                                                                    )
                                                                )
                                                            )
                                                            .WithStatements(
                                                                Statement(
                                                                    Invoke(
                                                                        Type(
                                                                            "System.Text.Json.JsonSerializer"
                                                                        ),
                                                                        "Serialize",
                                                                        Variable("writer"),
                                                                        AccessMember(
                                                                            Variable("value"),
                                                                            "_value"
                                                                        ),
                                                                        Variable("options")
                                                                    )
                                                                )
                                                            )
                                                    )
                                        )
                                )
                        );
                    }

                    var t = Compilation(namespaceDeclarations)
                        .NormalizeWhitespace()
                        .GetText(Encoding.UTF8)
                        .ToString();

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
