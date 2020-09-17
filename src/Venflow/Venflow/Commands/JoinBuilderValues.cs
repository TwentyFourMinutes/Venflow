using System.Collections.Generic;
using System.Text;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class JoinBuilderValues
    {
        internal Entity Root { get; }
        internal List<JoinPath> FullPath { get; }

        internal List<JoinOptions> Joins { get; }

        internal List<uint> UsedRelations { get; }

        private JoinPath _currentPath;

        private readonly bool _generateSql;

        internal JoinBuilderValues(Entity root, bool generateSql)
        {
            FullPath = new List<JoinPath>();
            Joins = new List<JoinOptions>();
            UsedRelations = new List<uint>();
            Root = root;
            _generateSql = generateSql;
        }

        internal void AddToPath(JoinOptions joinOptions, bool newFullPath)
        {
            if (newFullPath)
            {
                for (int i = FullPath.Count - 1; i >= 0; i--)
                {
                    var path = FullPath[i];

                    if (object.ReferenceEquals(path.JoinOptions.Join, joinOptions.Join))
                    {
                        _currentPath = path;

                        return;
                    }

                    var match = path.GetPath(joinOptions.Join);

                    if (match is { })
                    {
                        _currentPath = match;

                        return;
                    }
                }

                var newPath = new JoinPath(joinOptions, _generateSql ? new StringBuilder() : null);

                FullPath.Add(newPath);

                _currentPath = newPath;

                Joins.Add(joinOptions);
                UsedRelations.Add(joinOptions.Join.RelationId);
            }
            else
            {
                var match = _currentPath.GetPath(joinOptions.Join);

                if (match is { })
                {
                    _currentPath = match;

                    _currentPath.TrailingJoinPath.Add(match);

                    return;
                }

                if (_currentPath.TrailingJoinPath.Count == 0)
                {
                    var joinPath = new JoinPath(joinOptions, _currentPath.SqlJoins);

                    _currentPath.TrailingJoinPath.Add(joinPath);

                    _currentPath = joinPath;

                    Joins.Add(joinOptions);
                    UsedRelations.Add(joinOptions.Join.RelationId);
                }
                else
                {
                    var joinPath = new JoinPath(joinOptions, _generateSql ? _currentPath.GetNewSqlJoinsFromBasePath(_currentPath) : null);

                    _currentPath.TrailingJoinPath.Add(joinPath);

                    _currentPath = joinPath;

                    Joins.Add(joinOptions);
                    UsedRelations.Add(joinOptions.Join.RelationId);
                }
            }
        }

        internal void AppendColumnNamesAndJoins(StringBuilder? sqlColumns, StringBuilder sqlJoins)
        {
            for (int i = FullPath.Count - 1; i >= 0; i--)
            {
                FullPath[i].AppendColumnNamesAndJoins(sqlColumns, sqlJoins);
            }
        }
    }
}