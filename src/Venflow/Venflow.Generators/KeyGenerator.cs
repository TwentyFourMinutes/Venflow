using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Venflow.Generators
{
    /// <summary>
    /// A Source Generator that will generate a strongly typed id implementation.
    /// </summary>
    [Generator]
    public class KeyGenerator : ISourceGenerator
    {
        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

            Debug.WriteLine("Key-Generator: Initialized code generator.");
        }

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            Debug.WriteLine("Key-Generator: Executing code generator.");

            var references = context.Compilation.References.ToArray();

            if (!context.Compilation.ContainsAssembly(references, Assemblies.Venflow))
                throw new InvalidOperationException("The assembly 'Venflow' could not be found. Ensure that the 'Venflow' package is referenced.");

            var hasNewtonsoftReference = context.Compilation.ContainsAssembly(references, Assemblies.NewtonsoftJson);

            if (hasNewtonsoftReference &&
                !context.Compilation.ContainsAssembly(references, Assemblies.VenflowNewtonsoftJson))
            {
                context.AddResourceSource("NewtonsoftJsonKeyConverter");
            }

            var compilation = context.AddResourceSource("GeneratedKeyAttribute", true);

            var attributeSymbol = compilation.GetTypeByMetadataName("Venflow.GeneratedKeyAttribute");

            var comparableInterfaceType = compilation.GetTypeByMetadataName("System.IComparable");
            var convertibleInterfaceType = compilation.GetTypeByMetadataName("System.IConvertible");

            var specialTypeMembers = comparableInterfaceType.GetMembers().Union(convertibleInterfaceType.GetMembers()).OfType<IMethodSymbol>().ToArray();

            foreach (var declarationSyntax in ((SyntaxReceiver)context.SyntaxReceiver!).Candidates)
            {
                var semanticModel = compilation.GetSemanticModel(declarationSyntax.SyntaxTree);

                var underlyingKeyFullName = default(string);
                var underlyingKeyType = default(ITypeSymbol);

                foreach (var attributeSyntax in declarationSyntax.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var attributeType = semanticModel.GetTypeInfo(attributeSyntax, context.CancellationToken).Type;

                    if (!attributeType.Equals(attributeSymbol, SymbolEqualityComparer.Default) ||
                        attributeSyntax.ArgumentList.Arguments.Count != 1 ||
                        attributeSyntax.ArgumentList.Arguments[0].Expression is not TypeOfExpressionSyntax typeOfExpression)
                        continue;

                    underlyingKeyType = semanticModel.GetTypeInfo(typeOfExpression.Type, context.CancellationToken).Type;

                    underlyingKeyFullName = underlyingKeyType.ToString();

                    break;
                }

                if (underlyingKeyFullName is null ||
                    underlyingKeyType is null)
                    continue;

                var baseStruct = semanticModel.GetDeclaredSymbol(declarationSyntax);

                var namespaceText = baseStruct.ContainingNamespace;
                var typeArgumentName = baseStruct.TypeArguments[0].Name;
                var baseStructName = baseStruct.Name + "<" + typeArgumentName + ">";
                var baseStructXmlName = $"{baseStruct.Name}{{{typeArgumentName}}}";

                var parseTextBuilder = new StringBuilder();

                var underlyingKeyMembers = underlyingKeyType.GetMembers();

                if (underlyingKeyMembers.Length > 0)
                {
                    var comparableGenericType = underlyingKeyType.AllInterfaces.FirstOrDefault(x => x.ContainingNamespace + "." + x.MetadataName == "System.IComparable`1");

                    IEnumerable<ISymbol> tempSpecialTypeMembers;

                    if (comparableGenericType is null)
                    {
                        tempSpecialTypeMembers = specialTypeMembers;
                    }
                    else
                    {
                        tempSpecialTypeMembers = specialTypeMembers.Union(comparableGenericType.GetMembers());
                    }

                    foreach (var specialTypeMember in tempSpecialTypeMembers)
                    {
                        var methodImplementation = underlyingKeyType.FindImplementationForInterfaceMember(specialTypeMember);

                        if (methodImplementation is not IMethodSymbol methodSymbol)
                            continue;

                        AppendMethodText(methodSymbol);
                    }

                    foreach (var underlyingKeyMember in underlyingKeyMembers)
                    {
                        if (underlyingKeyMember is not IMethodSymbol methodSymbol ||
                            methodSymbol.DeclaredAccessibility != Accessibility.Public)
                            continue;

                        if (!methodSymbol.IsStatic ||
                            methodSymbol.Name is not "Parse" and not "TryParse" and not "ParseExact" and not "TryParseExact" ||
                            methodSymbol.ReturnType is null ||
                            methodSymbol.ReturnType.ContainingNamespace != underlyingKeyType.ContainingNamespace ||
                            (methodSymbol.ReturnType.Name != underlyingKeyType.Name &&
                            methodSymbol.ReturnType.ContainingNamespace + "." + methodSymbol.ReturnType.Name != typeof(bool).FullName))
                        {
                            continue;
                        }

                        AppendMethodText(methodSymbol);
                    }

                    void AppendMethodText(IMethodSymbol methodSymbol)
                    {
                        var methodName = methodSymbol.MethodKind == MethodKind.ExplicitInterfaceImplementation ? methodSymbol.Name.Substring(methodSymbol.Name.LastIndexOf(".") + 1, methodSymbol.Name.Length - methodSymbol.Name.LastIndexOf(".") - 1) : methodSymbol.Name;
                        var interfaceName = methodSymbol.MethodKind == MethodKind.ExplicitInterfaceImplementation ? methodSymbol.Name.Substring(0, methodSymbol.Name.LastIndexOf(".")) : string.Empty;

                        parseTextBuilder.Append(
$@"        /// <summary>
        /// Wraps around the <see cref=""{underlyingKeyType.Name}.{methodName}({string.Join(", ", methodSymbol.Parameters.Select(x => (x.RefKind == RefKind.Out ? "out " : string.Empty) + x.Type.ToString().Replace('<', '{').Replace('>', '}')))})""/> method.
        /// </summary>
        {(methodSymbol.MethodKind == MethodKind.ExplicitInterfaceImplementation ? string.Empty : "public ")}{(methodSymbol.IsStatic ? "static " : string.Empty)}{methodSymbol.ReturnType.Name} {methodSymbol.Name}({string.Join(", ", methodSymbol.Parameters.Select(x => (x.RefKind == RefKind.Out ? "out " : string.Empty) + x.Type.ToString() + " " + x.Name + (x.HasExplicitDefaultValue ? " = " + (x.ExplicitDefaultValue is null ? "default" : (x.Type.TypeKind == TypeKind.Enum ? x.Type.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(y => int.Equals(y.ConstantValue, x.ExplicitDefaultValue)).ToString() : x.ExplicitDefaultValue)) : string.Empty)))})
        {{
            return {(interfaceName != string.Empty ? "((" + interfaceName + ")" : string.Empty)}{(methodSymbol.IsStatic ? underlyingKeyType.Name : "_value")}{(interfaceName != string.Empty ? ")" : string.Empty)}.{methodName}({string.Join(", ", methodSymbol.Parameters.Select(x => (x.RefKind == RefKind.Out ? "out " : string.Empty) + x.Name))});
        }}

");
                    }
                }

                var implementedInterfacesText = string.Join(", ", new[] { "System.IComparable", "System.IComparable`1", "System.IConvertible" }.Where(x => underlyingKeyType.Interfaces.Any(y => y.ContainingNamespace + "." + y.MetadataName == x)).Select(x => x.Replace("`1", "<" + underlyingKeyFullName + ">")));

                implementedInterfacesText = implementedInterfacesText == string.Empty ? string.Empty : ", " + implementedInterfacesText;

                var keyText = @$"using System;
using Venflow;

namespace {namespaceText}
{{
    /// <summary>
    /// This is used to create strongly-typed ids.
    /// </summary>
    /// <typeparam name=""{typeArgumentName}"">They type of entity the key sits in.</typeparam>{(hasNewtonsoftReference ? Environment.NewLine + $"    [Newtonsoft.Json.JsonConverter(typeof(Venflow.Json.NewtonsoftJsonKeyConverter))]" : string.Empty)}
    [System.Text.Json.Serialization.JsonConverter(typeof(Venflow.Json.JsonKeyConverterFactory))]
    public readonly partial struct {baseStructName} : IKey<{typeArgumentName}, {underlyingKeyFullName}>, IEquatable<{baseStructName}>{implementedInterfacesText}
    {{
        private readonly {underlyingKeyFullName} _value;

        {underlyingKeyFullName} IKey<{typeArgumentName}, {underlyingKeyFullName}>.Value {{ get => _value; }}

        /// <summary>
        /// Instantiates a new <see cref=""{baseStructXmlName}""/> instance withe the provided value.
        /// </summary>
        /// <param name=""value"">The value which should represent the new <see cref=""{baseStructXmlName}""/> instance.</param>
        public Key({underlyingKeyFullName} value)
        {{
            _value = value;
        }}

{parseTextBuilder}        ///<inheritdoc/>
        public static implicit operator {underlyingKeyFullName}(in {baseStructName} key)
        {{
            return key._value;
        }}

        ///<inheritdoc/>
        public static implicit operator {baseStructName}(in {underlyingKeyFullName} value)
        {{
            return new {baseStructName}(value);
        }}

        ///<inheritdoc/>
        public static bool operator ==(in {baseStructName} a, in {baseStructName} b)
        {{
            return a.Equals(b);
        }}

        ///<inheritdoc/>
        public static bool operator !=(in {baseStructName} a, in {baseStructName} b)
        {{
            return !a.Equals(b);
        }}

        ///<inheritdoc/>
        public bool Equals({baseStructName} other)
        {{
            return other._value.Equals(this._value);
        }}

        ///<inheritdoc/>
        public override bool Equals(object? obj)
        {{
            if (obj is not {baseStructName} key)
            {{
                return false;
            }}

            return key._value.Equals(this._value);
        }}

        ///<inheritdoc/>
        public override int GetHashCode()
        {{
            return _value.GetHashCode();
        }}

        ///<inheritdoc/>
        public override string ToString()
        {{
            return _value.ToString();
        }}
    }}
}}
";
                context.AddSource(baseStruct.ContainingNamespace.ToString().Replace('.', '_') + "_" + baseStruct.MetadataName.Replace('`', '_') + "_generated", SourceText.From(keyText, Encoding.UTF8));
            }

            Debug.WriteLine("Key-Generator: Executed code generator.");
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            internal List<StructDeclarationSyntax> Candidates { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is StructDeclarationSyntax declarationSyntax &&
                    declarationSyntax.AttributeLists.Count > 0 &&
                    declarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    Candidates.Add(declarationSyntax);
                }
            }
        }
    }
}
