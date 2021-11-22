using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Sections
{
    internal partial class Entity
    {
        internal string? TableName { get; private set; }
        internal INamedTypeSymbol Symbol { get; }
        internal List<Column> Columns { get; }

        internal Entity(INamedTypeSymbol symbol, string tableName)
        {
            Symbol = symbol;
            TableName = tableName;
            Columns = new();

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
            BlockSyntax blockSyntax
        )
        {
            _ = propertySymbol;
            _ = entitySymbol;

            var entity = new Entity(entitySymbol, propertySymbol.Name);

            new FluentReader(entity, semanticModel, blockSyntax).Evaluate();

            return entity;
        }

        private class FluentReader : BulkFluentReader<Entity>
        {
            private Column? _currentColumn;
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
                string name,
                SeparatedSyntaxList<ArgumentSyntax> arguments
            )
            {
                switch (name)
                {
                    case "MapTable":
                        Value.TableName =
                            (
                                (LiteralExpressionSyntax)arguments.Single().Expression
                            ).Token.ValueText;
                        break;
                    case "Column":
                        var lambda = (SimpleLambdaExpressionSyntax)arguments.Single().Expression;
                        var columnName =
                            (
                                (MemberAccessExpressionSyntax)lambda.ExpressionBody!
                            ).Name.Identifier.Text;

                        if (!_columns.TryGetValue(columnName, out _currentColumn))
                            throw new InvalidOperationException();
                        break;
                    default:
                        return false;
                }

                return true;
            }

            protected override void ReadTail(
                string name,
                SeparatedSyntaxList<ArgumentSyntax> arguments
            )
            {
                if (_currentColumn is null)
                    return;

                switch (name)
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
        }
    }
}
