using System;
using System.Collections.Generic;
using System.Text;
using Venflow.Enums;
using Venflow.Modeling;

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
            for (int i = TrailingJoinPath.Count - 1; i >= 0; i--)
            {
                var joingingEntity = TrailingJoinPath[i];

                if (object.ReferenceEquals(TrailingJoinPath[i].JoinOptions.Join, foreignEntity))
                {
                    return joingingEntity;
                }
            }

            return null;
        }

        internal StringBuilder GetNewSqlJoinsFromBasePath(JoinPath fromPath)
        {
#if NET48
            return new StringBuilder().Append(fromPath.SqlJoins.ToString(), 0, fromPath._joinLength);
#else
            return new StringBuilder().Append(fromPath.SqlJoins, 0, fromPath._joinLength);
#endif
        }

        // TODO: Build this while generating
        internal void AppendColumnNamesAndJoins(StringBuilder? sqlColumns, StringBuilder sqlJoins)
        {
            if (sqlColumns is { })
            {
                sqlColumns.Append(", ");
                sqlColumns.Append(JoinOptions.Join.RightEntity.ExplicitColumnListString);
            }

            if (TrailingJoinPath.Count == 0)
            {
                sqlJoins.AppendLine();
                sqlJoins.Append(SqlJoins);
            }

            for (int i = TrailingJoinPath.Count - 1; i >= 0; i--)
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
                    throw new InvalidOperationException($"Invalid state '{JoinOptions.JoinBehaviour}' for the JoinBehaviour on entity {JoinOptions.Join.RightEntity.EntityName}");
            }

            SqlJoins.Append(JoinOptions.Join.RightEntity.TableName);
            SqlJoins.Append(" ON ");

            if (JoinOptions.Join.ForeignKeyLocation == ForeignKeyLocation.Left)
            {
                SqlJoins.Append(JoinOptions.Join.LeftEntity.TableName);
                SqlJoins.Append(".\"");
                SqlJoins.Append(JoinOptions.Join.ForeignKeyColumn.ColumnName);
                SqlJoins.Append("\" = ");
                SqlJoins.Append(JoinOptions.Join.RightEntity.TableName);
                SqlJoins.Append(".\"");
                SqlJoins.Append(JoinOptions.Join.RightEntity.GetPrimaryColumn().ColumnName);
            }
            else
            {
                SqlJoins.Append(JoinOptions.Join.RightEntity.TableName);
                SqlJoins.Append(".\"");
                SqlJoins.Append(JoinOptions.Join.ForeignKeyColumn.ColumnName);
                SqlJoins.Append("\" = ");
                SqlJoins.Append(JoinOptions.Join.LeftEntity.TableName);
                SqlJoins.Append(".\"");
                SqlJoins.Append(JoinOptions.Join.LeftEntity.GetPrimaryColumn().ColumnName);
            }

            SqlJoins.Append('"');
        }
    }
}