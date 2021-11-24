using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Sections.LambdaSorter
{
    internal class RelationBuilderValues
    {
        internal List<RelationPath> FlattenedPath { get; }

        private RelationPath _currentPath;

        private readonly List<RelationPath> _trailingPath;

        internal RelationBuilderValues()
        {
            _currentPath = default!;
            _trailingPath = new(1);
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
                for (var pathIndex = 0; pathIndex < _trailingPath.Count; pathIndex++)
                {
                    var path = _trailingPath[pathIndex];

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

                _trailingPath.Add(_currentPath);

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
