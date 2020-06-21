using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Venflow.Enums;
using Venflow.Modeling;
using Venflow.Models;

namespace Venflow.Commands
{

    internal class JoinBuilderValues
    {
        internal Entity Root { get; }
        internal List<JoinPath> FullPath { get; }

        internal List<JoinOptions> Joins { get; }

        private JoinPath _currentPath;

        internal JoinBuilderValues(Entity root)
        {
            FullPath = new List<JoinPath>();
            Joins = new List<JoinOptions>();
            Root = root;
        }

        internal void AddToPath(JoinOptions joinOptions, bool newFullPath)
        {
            if (newFullPath)
            {
                for (int i = 0; i < FullPath.Count; i++)
                {
                    var path = FullPath[i];

                    if (object.ReferenceEquals(path.JoinOptions.JoinWith, joinOptions.JoinWith))
                    {
                        _currentPath = path;

                        return;
                    }

                    var match = path.GetPath(joinOptions.JoinWith);

                    if (match is { })
                    {
                        _currentPath = match;

                        return;
                    }
                }

                var newPath = new JoinPath(joinOptions, new StringBuilder());

                FullPath.Add(newPath);

                _currentPath = newPath;

                Joins.Add(joinOptions);
            }
            else
            {
                var match = _currentPath.GetPath(joinOptions.JoinWith);

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
                }
                else
                {
                    var joinPath = new JoinPath(joinOptions, _currentPath.GetNewSqlJoinsFromBasePath(_currentPath));

                    _currentPath.TrailingJoinPath.Add(joinPath);

                    _currentPath = joinPath;

                    Joins.Add(joinOptions);
                }
            }
        }

        internal void AppendColumnNamesAndJoins(StringBuilder sqlColumns, StringBuilder sqlJoins)
        {
            for (int i = 0; i < FullPath.Count; i++)
            {
                FullPath[i].AppendColumnNamesAndJoins(sqlColumns, sqlJoins);
            }
        }
    }
}