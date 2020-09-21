using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Npgsql.Schema;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal readonly struct QueryCacheKey
    {
        internal readonly Entity Entity;
        internal readonly Type ReturnType;
        internal readonly EntityRelation[]? Relations;
        internal readonly List<NpgsqlDbColumn> ColumnSchema;
        internal readonly bool IsChangeTracking;

        public QueryCacheKey(Entity entity, Type returnType, EntityRelation[]? relations, List<NpgsqlDbColumn> columnSchema, bool isChangeTracking)
        {
            Entity = entity;
            ReturnType = returnType;
            Relations = relations;
            ColumnSchema = columnSchema;
            IsChangeTracking = isChangeTracking;
        }

        public bool Equals(
#if !NET48
            [AllowNull] 
#endif
            QueryCacheKey y)
        {
            if (y.ReturnType != ReturnType ||
                y.IsChangeTracking != IsChangeTracking ||
                y.ColumnSchema.Count != ColumnSchema.Count)
                return false;

            var columnSchemaSpan = ColumnSchema.AsSpan();
            var foreignColumnSchemaSpan = y.ColumnSchema.AsSpan();

            for (int columnIndex = columnSchemaSpan.Length - 1; columnIndex >= 0; columnIndex--)
            {
                if (columnSchemaSpan[columnIndex].ColumnName != foreignColumnSchemaSpan[columnIndex].ColumnName)
                    return false;
            }

            return true;
        }

        public new int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(ReturnType);

            var columnSchemaSpan = ColumnSchema.AsSpan();

            if (Relations is { })
            {
                var joinIndex = 0;

                var flattenedPathSpan = Relations.AsSpan();

                Entity nextJoin = Entity;
                string? nextJoinPKName = Entity.GetPrimaryColumn().ColumnName;

                for (int columnIndex = 0, max = columnSchemaSpan.Length; columnIndex < max; columnIndex++)
                {
                    var columnName = columnSchemaSpan[columnIndex].ColumnName;

                    if (columnName == nextJoinPKName)
                    {
                        hashCode.Add(nextJoin.EntityName);

                        if (flattenedPathSpan.Length == joinIndex)
                            break;

                        nextJoin = flattenedPathSpan[joinIndex].RightEntity;
                        nextJoinPKName = nextJoin.GetPrimaryColumn().ColumnName;

                        joinIndex++;
                    }

                    hashCode.Add(columnName);
                }
            }

            hashCode.Add(IsChangeTracking);

            return hashCode.ToHashCode();
        }
    }
}