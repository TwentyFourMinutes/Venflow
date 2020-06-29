using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using Venflow.Enums;
using Venflow.Modeling;
using Venflow.Models;

namespace Venflow.Commands
{
    internal class JoinPath
    {
        internal JoinOptions JoinOptions { get; }

        internal List<JoinPath> TrailingJoinPath { get; }

        internal StringBuilder? SqlJoins { get; }

        private readonly int _joinLength;

        internal JoinPath(JoinOptions joinOptions, StringBuilder? sqlJoins)
        {
            JoinOptions = joinOptions;
            TrailingJoinPath = new List<JoinPath>();
            SqlJoins = sqlJoins;

            if (sqlJoins is { })
            {
                AppendJoin();

                _joinLength = SqlJoins.Length;
            }
        }

        internal JoinPath? GetPath(EntityRelation foreignEntity)
        {
            for (int i = 0; i < TrailingJoinPath.Count; i++)
            {
                var joingingEntity = TrailingJoinPath[i];

                if (object.ReferenceEquals(TrailingJoinPath[i].JoinOptions.JoinWith, foreignEntity))
                {
                    return joingingEntity;
                }
            }

            return null;
        }

        internal StringBuilder GetNewSqlJoinsFromBasePath(JoinPath fromPath)
            => new StringBuilder()
                .Append(fromPath.SqlJoins, 0, fromPath._joinLength);

        // TODO: Build this while generating
        internal void AppendColumnNamesAndJoins(StringBuilder sqlColumns, StringBuilder sqlJoins)
        {
            sqlColumns.Append(", ");
            sqlColumns.Append(JoinOptions.JoinWith.RightEntity.PrimaryKeyPrefiexColumnListString);

            if (TrailingJoinPath.Count == 0)
            {
                sqlJoins.AppendLine();
                sqlJoins.Append(SqlJoins);
            }

            for (int i = 0; i < TrailingJoinPath.Count; i++)
            {
                TrailingJoinPath[i].AppendColumnNamesAndJoins(sqlColumns, sqlJoins);
            }
        }

        private void AppendJoin()
        {
            if (SqlJoins.Length > 0)
                SqlJoins.AppendLine();

            switch (JoinOptions.JoinBehaviour)
            {
                case JoinBehaviour.InnerJoin:
                    SqlJoins.Append("INNER JOIN ");
                    break;
                case JoinBehaviour.LeftJoin:
                    SqlJoins.Append("LEFT JOIN ");
                    break;
                case JoinBehaviour.RightJoin:
                    SqlJoins.Append("RIGHT JOIN ");
                    break;
                case JoinBehaviour.FullJoin:
                    SqlJoins.Append("FULL JOIN ");
                    break;
                default:
                    throw new InvalidOperationException($"Invalid state '{JoinOptions.JoinBehaviour}' for the JoinBehaviour on entity {JoinOptions.JoinWith.RightEntity.EntityName}");
            }

            SqlJoins.Append(JoinOptions.JoinWith.RightEntity.TableName);
            SqlJoins.Append(" AS ");
            SqlJoins.Append(JoinOptions.JoinWith.RightEntity.RawTableName);
            SqlJoins.Append(" ON ");
            SqlJoins.Append(JoinOptions.JoinWith.RightEntity.RawTableName);
            SqlJoins.Append(".\"");
            SqlJoins.Append(JoinOptions.JoinWith.ForeignKeyColumn.ColumnName);
            SqlJoins.Append("\" = ");
            SqlJoins.Append(JoinOptions.JoinFrom.RawTableName);
            SqlJoins.Append(".\"");
            SqlJoins.Append(JoinOptions.JoinFrom.GetPrimaryColumn().ColumnName);
            SqlJoins.Append('"');
        }
    }
}