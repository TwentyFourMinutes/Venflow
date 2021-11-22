using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Models
{
    internal class EntityRelation
    {
        internal bool IsProcessed { get; set; }

        internal INamedTypeSymbol LeftEntitySymbol { get; set; } = null!;
        internal IPropertySymbol? LeftNavigationProperty { get; set; }

        internal INamedTypeSymbol RightEntitySymbol { get; set; } = null!;
        internal IPropertySymbol? RightNavigationProperty { get; set; }

        internal IPropertySymbol ForeignKeySymbol { get; set; } = null!;
        internal RelationType RelationType { get; set; }
        internal ForeignKeyLocation ForeignKeyLocation { get; set; }

        internal EntityRelation GetMirror()
        {
            return new EntityRelation
            {
                LeftEntitySymbol = RightEntitySymbol,
                LeftNavigationProperty = RightNavigationProperty,
                RightEntitySymbol = LeftEntitySymbol,
                RightNavigationProperty = LeftNavigationProperty,
                ForeignKeySymbol = ForeignKeySymbol,
                RelationType = RelationType.GetMirror(),
                ForeignKeyLocation =
                    ForeignKeyLocation is ForeignKeyLocation.Left
                        ? ForeignKeyLocation.Right
                        : ForeignKeyLocation.Left,
                IsProcessed = IsProcessed,
            };
        }
    }
}
