using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using static Reflow.Internal.CSharpCodeGenerator;

namespace Reflow.Internal.Analyzer.Emitters
{
    internal static class OrderableGeneratorAttributeEmitter
    {
        internal static SourceText Emit() =>
            File("Reflow.Internal")
                .WithMembers(
                    Class("OrderableGeneratorAttribute", CSharpModifiers.Public)
                        .WithBase(Type(nameof(System.Attribute)))
                        .WithAttributes(
                            Attribute(nameof(AttributeUsageAttribute))
                                .WithArguments(
                                    AccessMember(
                                        IdentifierName(nameof(AttributeTargets)),
                                        nameof(AttributeTargets.Class)
                                    )
                                ),
                            Attribute(nameof(ConditionalAttribute))
                                .WithArguments(Constant("REFLOW_INTERNAL"))
                        )
                )
                .GetText();
    }
}
