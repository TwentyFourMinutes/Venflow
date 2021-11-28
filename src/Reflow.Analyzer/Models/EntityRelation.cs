using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Models
{
    internal class EntityRelation
    {
        internal int Id { get; set; }
        internal bool IsProcessed { get; set; }

        internal INamedTypeSymbol LeftEntitySymbol { get; set; } = null!;
        internal IPropertySymbol? LeftNavigationProperty { get; set; }
        internal bool IsLeftNavigationPropertyNullable =>
            LeftNavigationProperty is not null
            && LeftNavigationProperty.NullableAnnotation == NullableAnnotation.Annotated;
        internal bool IsLeftNavigationPropertyInitialized =>
            LeftNavigationProperty!.SetMethod is null
            || LeftNavigationProperty!.SetMethod.DeclaredAccessibility != Accessibility.Public;

        internal INamedTypeSymbol RightEntitySymbol { get; set; } = null!;
        internal IPropertySymbol? RightNavigationProperty { get; set; }
        internal bool IsRightNavigationPropertyNullable =>
            RightNavigationProperty is not null
            && RightNavigationProperty.NullableAnnotation == NullableAnnotation.Annotated;
        internal bool IsRightNavigationPropertyInitialized =>
            RightNavigationProperty!.SetMethod is null
            || RightNavigationProperty!.SetMethod.DeclaredAccessibility != Accessibility.Public;

        internal IPropertySymbol ForeignKeySymbol { get; set; } = null!;
        internal RelationType RelationType { get; set; }
        internal ForeignKeyLocation ForeignKeyLocation { get; set; }

        internal EntityRelation Mirror { get; private set; } = null!;

        internal EntityRelation CreateMirror()
        {
            return Mirror = new EntityRelation
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
                Mirror = this,
            };
        }
    }
}
