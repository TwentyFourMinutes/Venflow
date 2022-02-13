using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Models
{
    internal class Entity
    {
        internal string? TableName { get; private set; }
        internal INamedTypeSymbol Symbol { get; }
        internal List<Column> Columns { get; }
        internal List<EntityRelation> Relations { get; set; }

        internal Entity(INamedTypeSymbol symbol, string tableName)
        {
            Symbol = symbol;
            TableName = tableName;
            Columns = new();
            Relations = new();

            InitializeColumns();
        }

        private void InitializeColumns()
        {
            var members = Symbol.GetMembers();

            for (var memberIndex = 0; memberIndex < members.Length; memberIndex++)
            {
                var member = members[memberIndex];

                if (
                    member is not IPropertySymbol propertySymbol
                    || propertySymbol.DeclaredAccessibility is not Accessibility.Public
                    || propertySymbol.GetMethod is null
                    || propertySymbol.GetMethod.DeclaredAccessibility is not Accessibility.Public
                )
                {
                    continue;
                }

                Columns.Add(
                    new Column(
                        propertySymbol.Name,
                        (INamedTypeSymbol)propertySymbol.Type,
                        propertySymbol.IsVirtual
                    )
                );
            }
        }

        internal static Entity Construct(
            SemanticModel semanticModel,
            IPropertySymbol propertySymbol,
            INamedTypeSymbol entitySymbol,
            BlockSyntax? blockSyntax
        )
        {
            _ = propertySymbol;
            _ = entitySymbol;

            var entity = new Entity(entitySymbol, propertySymbol.Name);

            if (blockSyntax is not null)
                new FluentReader(entity, semanticModel, blockSyntax).Evaluate();

            return entity;
        }

        private class FluentReader : BulkFluentSyntaxReader<Entity>
        {
            private Column? _currentColumn;
            private EntityRelation? _currentRelation;
            private readonly Dictionary<string, Column> _columns;

            internal FluentReader(
                Entity entity,
                SemanticModel semanticModel,
                BlockSyntax blockSyntax
            ) : base(entity, semanticModel, blockSyntax)
            {
                _columns = entity.Columns.ToDictionary(x => x.ColumnName);
            }

            protected override bool ValidateHead(
                IMethodSymbol methodSymbol,
                SeparatedSyntaxList<ArgumentSyntax> arguments
            )
            {
                switch (methodSymbol.Name)
                {
                    case "MapTable":
                    {
                        Value.TableName =
                            (
                                (LiteralExpressionSyntax)arguments.Single().Expression
                            ).Token.ValueText;
                        break;
                    }
                    case "Column":
                    {
                        var lambda = (SimpleLambdaExpressionSyntax)arguments.Single().Expression;
                        var columnName =
                            (
                                (MemberAccessExpressionSyntax)lambda.ExpressionBody!
                            ).Name.Identifier.Text;

                        if (!_columns.TryGetValue(columnName, out _currentColumn))
                            throw new InvalidOperationException();
                        break;
                    }
                    case "Ignore":
                    {
                        var lambda = (SimpleLambdaExpressionSyntax)arguments.Single().Expression;
                        var columnName =
                            (
                                (MemberAccessExpressionSyntax)lambda.ExpressionBody!
                            ).Name.Identifier.Text;

                        if (!_columns.TryGetValue(columnName, out var column))
                            throw new InvalidOperationException();

                        _columns.Remove(columnName);
                        Value.Columns.Remove(column);
                        break;
                    }
                    case string methodName
                          when methodName is "HasOne" or "HasMany" && arguments.Count is 0:
                    {
                        _currentRelation = new EntityRelation
                        {
                            RelationType = methodName is "HasOne"
                                ? RelationType.OneToOne
                                : RelationType.ManyToOne,
                            LeftEntitySymbol = Value.Symbol,
                            RightEntitySymbol = (INamedTypeSymbol)methodSymbol.TypeArguments[0],
                        };
                        break;
                    }
                    case string methodName
                          when methodName is "HasOne" or "HasMany" && arguments.Count is 1:
                    {
                        var memberAccess = GetMemberAccessFromLambda(arguments.Single());
                        var propertyName = memberAccess.Name.Identifier.Text;

                        if (_columns.TryGetValue(propertyName, out var column))
                        {
                            _columns.Remove(propertyName);
                            Value.Columns.Remove(column);
                        }

                        _currentRelation = new EntityRelation
                        {
                            RelationType = methodName is "HasOne"
                                ? RelationType.OneToOne
                                : RelationType.ManyToOne,
                            LeftEntitySymbol = Value.Symbol,
                            LeftNavigationProperty = (IPropertySymbol)SemanticModel.GetSymbolInfo(
                                memberAccess
                            ).Symbol!,
                            RightEntitySymbol = (INamedTypeSymbol)methodSymbol.TypeArguments[0],
                        };
                        break;
                    }
                    default:
                        return false;
                }

                return true;
            }

            protected override void ReadTail(
                IMethodSymbol methodSymbol,
                SeparatedSyntaxList<ArgumentSyntax> arguments
            )
            {
                if (_currentColumn is not null)
                {
                    switch (methodSymbol.Name)
                    {
                        case "IsId":
                            throw new NotImplementedException();
                        case "HasName":
                            _currentColumn.ColumnName =
                                (
                                    (LiteralExpressionSyntax)arguments.Single().Expression
                                ).Token.ValueText;
                            break;
                        case "HasType":
                            throw new NotImplementedException();
                        case "HasDefault":
                            throw new NotImplementedException();
                    }
                }
                else if (_currentRelation is not null)
                {
                    switch (methodSymbol.Name)
                    {
                        case string methodName
                              when methodName is "WithOne" or "WithMany" && arguments.Count is 0:
                        {
                            if (methodName is "WithOne")
                            {
                                _currentRelation.RelationType = RelationType.OneToMany;
                            }
                            break;
                        }
                        case string methodName
                              when methodName is "WithOne" or "WithMany" && arguments.Count is 1:
                        {
                            var memberAccess = GetMemberAccessFromLambda(arguments.Single());

                            if (methodName is "WithOne")
                            {
                                _currentRelation.RelationType = RelationType.OneToMany;
                            }

                            _currentRelation.RightNavigationProperty =
                                (IPropertySymbol)SemanticModel.GetSymbolInfo(memberAccess).Symbol!;
                            break;
                        }
                        case "UsingForeignKey":
                        {
                            var memberAccess = GetMemberAccessFromLambda(arguments.Single());

                            _currentRelation.ForeignKeySymbol =
                                (IPropertySymbol)SemanticModel.GetSymbolInfo(memberAccess).Symbol!;

                            _currentRelation.ForeignKeyLocation =
                                _currentRelation.ForeignKeySymbol.ContainingType.Equals(
                                    _currentRelation.LeftEntitySymbol,
                                    SymbolEqualityComparer.Default
                                )
                                  ? ForeignKeyLocation.Left
                                  : ForeignKeyLocation.Right;
                            break;
                        }
                    }
                }
            }

            protected override bool ValidateTail()
            {
                _currentColumn = null!;

                if (_currentRelation is not null)
                {
                    Value.Relations.Add(_currentRelation);

                    _currentRelation = null!;
                }

                return true;
            }

            private static MemberAccessExpressionSyntax GetMemberAccessFromLambda(
                ArgumentSyntax argumentSyntax
            )
            {
                var lambda = (SimpleLambdaExpressionSyntax)argumentSyntax.Expression;
                var memberAccess = (MemberAccessExpressionSyntax)lambda.ExpressionBody!;

                if (memberAccess.Expression is not IdentifierNameSyntax)
                    throw new InvalidOperationException();

                return memberAccess;
            }
        }
    }
}
