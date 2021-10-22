using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.SyntaxGenerator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Reflow.Analyzer.SyntaxGenerator.CSharpSyntaxGenerator;

namespace Reflow.Analyzer.Database.Emitters
{
    internal static class EntityProxyEmitter
    {
        internal static SourceText Emit(Dictionary<string, List<IPropertySymbol>> updatableEntites)
        {
            var members = new MemberDeclarationSyntax[updatableEntites.Count];

            var memberIndex = 0;

            foreach (var updatableEntity in updatableEntites)
            {
                var proxyTypeName = "__" + updatableEntity.Key.Replace('.', '_');
                var proxyMembers = new List<MemberDeclarationSyntax>();

                var propertyCount = updatableEntity.Value.Count;

                var numericType = (propertyCount + 1) switch
                {
                    < sizeof(byte) * 8 => SyntaxKind.ByteKeyword,
                    < sizeof(ushort) * 8 => SyntaxKind.UShortKeyword,
                    < sizeof(uint) * 8 => SyntaxKind.UIntKeyword,
                    _ => SyntaxKind.ULongKeyword,
                };

                if (numericType != SyntaxKind.ULongKeyword)
                {
                    proxyMembers.Add(
                        Field(
                            Variable("_changes", PredefinedType(Token(numericType))),
                            SyntaxKind.PrivateKeyword
                        )
                    );
                }
                else
                    throw new NotImplementedException();

                proxyMembers.Add(
                    Constructor(proxyTypeName, SyntaxKind.PublicKeyword)
                        .WithParameters(
                            (
                                Type: PredefinedType(Token(SyntaxKind.BoolKeyword)),
                                Name: "trackChanges",
                                Default: SyntaxKind.TrueLiteralExpression
                            )
                        )
                        .WithStatements(
                            IfStatement(
                                IdentifierName("trackChanges"),
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.OrAssignmentExpression,
                                        IdentifierName("_changes"),
                                        Constant(1)
                                    )
                                )
                            )
                        )
                );

                for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
                {
                    var property = updatableEntity.Value[propertyIndex];

                    proxyMembers.Add(
                        Property(
                                Type(property.Type.GetFullName()),
                                property.Name,
                                SyntaxKind.PublicKeyword,
                                SyntaxKind.OverrideKeyword
                            )
                            .WithAccessors(
                                GetAccessor()
                                    .WithStatements(
                                        ReturnStatement(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                BaseExpression(),
                                                IdentifierName(property.Name)
                                            )
                                        )
                                    ),
                                SetAccessor()
                                    .WithStatements(
                                        ExpressionStatement(
                                            AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    BaseExpression(),
                                                    IdentifierName(property.Name)
                                                ),
                                                IdentifierName("value")
                                            )
                                        ),
                                        IfStatement(
                                            BinaryExpression(
                                                SyntaxKind.NotEqualsExpression,
                                                ParenthesizedExpression(
                                                    BinaryExpression(
                                                        SyntaxKind.BitwiseAndExpression,
                                                        IdentifierName("_changes"),
                                                        Constant(1)
                                                    )
                                                ),
                                                Constant(0)
                                            ),
                                            ExpressionStatement(
                                                AssignmentExpression(
                                                    SyntaxKind.OrAssignmentExpression,
                                                    IdentifierName("_changes"),
                                                    Constant(1 << (propertyIndex + 1))
                                                )
                                            )
                                        )
                                    )
                            )
                    );
                }

                members[memberIndex++] = Class(
                        proxyTypeName,
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.SealedKeyword
                    )
                    .WithBase(Type(updatableEntity.Key))
                    .WithMembers(proxyMembers);
            }

            return File(
                usings: System.Array.Empty<string>(),
                namespaceName: "Reflow.Proxies",
                members: members
            );
        }
    }
}
