using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class EntityProxyEmitter
    {
        internal static SourceText Emit(Dictionary<ITypeSymbol, Entity> updatableEntites)
        {
            var members = new List<SyntaxNode>();

            foreach (var updatableEntity in updatableEntites)
            {
                var proxyTypeName = "__" + updatableEntity.Key.Name.Replace('.', '_');
                var proxyMembers = new List<MemberDeclarationSyntax>();

                updatableEntity.Value.ProxyName = "Reflow.Proxies." + proxyTypeName;

                var updatableProperties = new List<Column>();

                for (
                    var columnIndex = 0;
                    columnIndex < updatableEntity.Value.Columns.Count;
                    columnIndex++
                )
                {
                    var column = updatableEntity.Value.Columns[columnIndex];

                    if (column.IsUpdatable)
                    {
                        updatableProperties.Add(column);
                    }
                }

                var propertyCount = updatableProperties.Count;

                var numericType = BitUtilities.GetTypeBySize(propertyCount + 1);

                if (numericType != TypeCode.UInt64)
                {
                    proxyMembers.Add(Field("_changes", Type(numericType), CSharpModifiers.Private));
                }
                else
                    throw new NotImplementedException();

                proxyMembers.Add(
                    Constructor(proxyTypeName, CSharpModifiers.Public)
                        .WithParameters(
                            Parameter("trackChanges", Type(TypeCode.Boolean))
                                .WithDefault(Constant(false))
                        )
                        .WithStatements(
                            If(Variable("trackChanges"), SetBit(This(), Variable("_changes"), 0))
                        )
                );

                for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
                {
                    var property = updatableProperties[propertyIndex];

                    proxyMembers.Add(
                        Property(
                                property.PropertyName,
                                Type(property.Type),
                                CSharpModifiers.Public | CSharpModifiers.Override
                            )
                            .WithGetAccessor(Return(AccessMember(Base(), property.PropertyName)))
                            .WithSetAccessor(
                                AssignMember(Base(), property.PropertyName, Value()),
                                If(
                                    IsBitSet(Variable("_changes"), Type(numericType), 0),
                                    SetBit(This(), Variable("_changes"), propertyIndex + 1)
                                )
                            )
                    );
                }

                proxyMembers.Add(
                    Method("GetSectionChanges", Type(numericType), CSharpModifiers.Public)
                        .WithParameters(Parameter("sectionIndex", Type(typeof(byte))))
                        .WithStatements(
                            Switch(
                                Variable("sectionIndex"),
                                Case(
                                    Constant(0),
                                    Return(
                                        Cast(
                                            Type(numericType),
                                            Parenthesis(
                                                ShiftRight(Variable("_changes"), Constant(1))
                                            )
                                        )
                                    )
                                ),
                                DefaultCase(Throw(Instance(Type(typeof(ArgumentException)))))
                            )
                        )
                );

                proxyMembers.Add(
                    Method("TrackChanges", Void(), CSharpModifiers.Public)
                        .WithStatements(SetBit(This(), Variable("_changes"), 0))
                );

                members.Add(
                    Class(proxyTypeName, CSharpModifiers.Public | CSharpModifiers.Sealed)
                        .WithBase(Type(updatableEntity.Key))
                        .WithMembers(proxyMembers)
                );
            }

            return File("Reflow.Proxies").WithMembers(members).GetText();
        }
    }
}
