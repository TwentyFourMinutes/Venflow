using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Internal;
using static Reflow.Internal.CSharpCodeGenerator;

namespace Reflow.Analyzer.Database.Emitters
{
    internal static class EntityProxyEmitter
    {
        internal static SourceText Emit(Dictionary<string, List<IPropertySymbol>> updatableEntites)
        {
            var members = new SyntaxList<SyntaxNode>();

            foreach (var updatableEntity in updatableEntites)
            {
                var proxyTypeName = "__" + updatableEntity.Key.Replace('.', '_');
                var proxyMembers = new SyntaxList<MemberDeclarationSyntax>();

                var propertyCount = updatableEntity.Value.Count;

                var numericType = (propertyCount + 1) switch
                {
                    <= sizeof(byte) * 8 => TypeCode.Byte,
                    <= sizeof(ushort) * 8 => TypeCode.UInt16,
                    <= sizeof(uint) * 8 => TypeCode.UInt32,
                    _ => TypeCode.UInt64,
                };

                if (numericType != TypeCode.UInt64)
                {
                    proxyMembers = proxyMembers.Add(
                        Field("_changes", Type(numericType), CSharpModifiers.Private)
                    );
                }
                else
                    throw new NotImplementedException();

                proxyMembers = proxyMembers.Add(
                    Constructor(proxyTypeName, CSharpModifiers.Public)
                        .WithParameters(
                            Parameter("trackChanges", Type(TypeCode.Boolean))
                                .WithDefault(SyntaxKind.FalseLiteralExpression)
                        )
                        .WithStatements(
                            If(
                                IdentifierName("trackChanges"),
                                AssignMember(
                                    This(),
                                    IdentifierName("_changes"),
                                    Constant(1),
                                    SyntaxKind.OrAssignmentExpression
                                )
                            )
                        )
                );

                for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
                {
                    var property = updatableEntity.Value[propertyIndex];

                    proxyMembers = proxyMembers.Add(
                        Property(
                                property.Name,
                                Type(property.Type),
                                CSharpModifiers.Public | CSharpModifiers.Override
                            )
                            .WithGetAccessor(Return(AccessMember(Base(), property.Name)))
                            .WithSetAccessor(
                                AssignMember(Base(), property.Name, Value()),
                                If(
                                    IsBitSet(
                                        IdentifierName("_changes"),
                                        Type(numericType),
                                        Constant(1)
                                    ),
                                    AssignMember(
                                        This(),
                                        IdentifierName("_changes"),
                                        Constant(1 << (propertyIndex + 1)),
                                        SyntaxKind.OrAssignmentExpression
                                    )
                                )
                            )
                    );
                }

                members = members.Add(
                    Class(proxyTypeName, CSharpModifiers.Public | CSharpModifiers.Sealed)
                        .WithBase(Type(updatableEntity.Key))
                        .WithMembers(proxyMembers)
                );
            }

            return File("Reflow.Proxies").WithMembers(members).GetText();
        }
    }
}
