using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Sections.LambdaSorter
{
    internal class RelationPath
    {
        internal INamedTypeSymbol LeftEntitySymbol { get; }
        internal INamedTypeSymbol RightEntitySymbol { get; }
        internal IPropertySymbol NavigationSymbol { get; }
        internal List<RelationPath> TrailingPath { get; }

        internal RelationPath(
            INamedTypeSymbol leftEntitySymbol,
            INamedTypeSymbol rightEntitySymbol,
            IPropertySymbol navigationSymbol
        )
        {
            LeftEntitySymbol = leftEntitySymbol;
            RightEntitySymbol = rightEntitySymbol;
            NavigationSymbol = navigationSymbol;

            TrailingPath = new(1);
        }

        internal RelationPath AddToPath(
            INamedTypeSymbol leftEntitySymbol,
            INamedTypeSymbol rightEntitySymbol,
            IPropertySymbol navigationSymbol,
            out bool isNew
        )
        {
            for (var pathIndex = 0; pathIndex < TrailingPath.Count; pathIndex++)
            {
                var trailingPath = TrailingPath[pathIndex];

                if (trailingPath.Equals(leftEntitySymbol, rightEntitySymbol, navigationSymbol))
                {
                    isNew = false;

                    return trailingPath;
                }
            }

            var path = new RelationPath(leftEntitySymbol, rightEntitySymbol, navigationSymbol);

            TrailingPath.Add(path);

            isNew = true;

            return path;
        }

        public bool Equals(
            INamedTypeSymbol leftEntitySymbol,
            INamedTypeSymbol rightEntitySymbol,
            IPropertySymbol navigationSymbol
        )
        {
            return this.LeftEntitySymbol.Equals(leftEntitySymbol, SymbolEqualityComparer.Default)
                && this.RightEntitySymbol.Equals(rightEntitySymbol, SymbolEqualityComparer.Default)
                && this.NavigationSymbol.Equals(navigationSymbol, SymbolEqualityComparer.Default);
        }
    }
}
