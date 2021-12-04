using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Models
{
    internal class RelationBuilderValues
    {
        internal List<RelationPath> FlattenedPath { get; }
        internal List<RelationPath> TrailingPath { get; }

        private RelationPath _currentPath;

        internal RelationBuilderValues()
        {
            _currentPath = default!;
            TrailingPath = new(1);
            FlattenedPath = new(4);
        }

        internal void AddToPath(
            INamedTypeSymbol leftEntitySymbol,
            INamedTypeSymbol rightEntitySymbol,
            IPropertySymbol navigationSymbol,
            bool isNested
        )
        {
            if (isNested)
            {
                for (var pathIndex = 0; pathIndex < TrailingPath.Count; pathIndex++)
                {
                    var path = TrailingPath[pathIndex];

                    if (path.Equals(leftEntitySymbol, rightEntitySymbol, navigationSymbol))
                    {
                        _currentPath = path;

                        return;
                    }
                }

                _currentPath = new RelationPath(
                    leftEntitySymbol,
                    rightEntitySymbol,
                    navigationSymbol
                );

                TrailingPath.Add(_currentPath);

                FlattenedPath.Add(_currentPath);
            }
            else
            {
                _currentPath = _currentPath.AddToPath(
                    leftEntitySymbol,
                    rightEntitySymbol,
                    navigationSymbol,
                    out var isNew
                );

                if (isNew)
                {
                    FlattenedPath.Add(_currentPath);
                }
            }
        }
    }
}
