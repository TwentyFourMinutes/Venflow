using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Venflow.Keys.Generated
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

            Debug.WriteLine("Strongly-Typed-Id-Generator: Initialized code generator.");
        }

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            Debug.WriteLine("Strongly-Typed-Id-Generator: Executing code generator.");

            var baseKeyType = context.Compilation.GetTypeByMetadataName("Venflow.IKey`2");

            if (baseKeyType is null)
                throw new InvalidOperationException("The type 'Venflow.IKey`2' could not be found. Ensure that the 'Venflow' package is referenced.");

            const string attributeText = @"using System;
using System.Reflection;

namespace Venflow.Keys
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratedKeyAttribute : Attribute
    {
        private Type _keyType;

        public GeneratedKeyAttribute(Type keyType)
        {
            _keyType = keyType;
        }
    }
}";

            context.AddSource("GeneratedKeyAttribute", SourceText.From(attributeText, Encoding.UTF8));

            var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));
            var attributeSymbol = compilation.GetTypeByMetadataName("Venflow.Keys.GeneratedKeyAttribute");

            foreach (var declarationSyntax in ((SyntaxReceiver)context.SyntaxReceiver!).Candidates)
            {
                var semanticModel = compilation.GetSemanticModel(declarationSyntax.SyntaxTree);

                var underlyingKeyFullName = default(string);

                foreach (var attributeSyntax in declarationSyntax.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var attributeType = semanticModel.GetTypeInfo(attributeSyntax, context.CancellationToken).Type;

                    if (!attributeType.Equals(attributeSymbol, SymbolEqualityComparer.Default) ||
                        attributeSyntax.ArgumentList.Arguments.Count != 1 ||
                        attributeSyntax.ArgumentList.Arguments[0].Expression is not TypeOfExpressionSyntax typeOfExpression)
                        continue;

                    underlyingKeyFullName = semanticModel.GetTypeInfo(typeOfExpression.Type, context.CancellationToken).Type.ToString();

                    break;
                }

                if (underlyingKeyFullName is null)
                    break;

                var baseStruct = semanticModel.GetDeclaredSymbol(declarationSyntax);

                var namespaceText = baseStruct.ContainingNamespace;
                var typeArgumentName = baseStruct.TypeArguments[0].Name;
                var baseStructName = baseStruct.Name + "<" + typeArgumentName + ">";
                var baseStructXmlName = $"{baseStruct.Name}{{{typeArgumentName}}}";

                var keyText = @$"using System;
using Venflow;

namespace {namespaceText}
{{
    /// <summary>
    /// This is used to create strongly-typed ids.
    /// </summary>
    /// <typeparam name=""{typeArgumentName}"">They type of entity the key sits in.</typeparam>
    public readonly partial struct {baseStructName} : IKey<{typeArgumentName}, {underlyingKeyFullName}>, IEquatable<{baseStructName}>
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

        ///<inheritdoc/>
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
                context.AddSource(baseStruct.Name + "_generated", SourceText.From(keyText, Encoding.UTF8));
            }

            Debug.WriteLine("Strongly-Typed-Id-Generator: Executed code generator.");
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
