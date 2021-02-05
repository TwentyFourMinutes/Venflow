using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Npgsql.Schema;
using Venflow.Dynamic;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal readonly struct QueryCacheKey
    {
        private readonly Entity _entity;
        private readonly Type _returnType;
        private readonly EntityRelation[]? _relations;
        private readonly List<NpgsqlDbColumn> _columnSchema;
        private readonly bool _isChangeTracking;

        public QueryCacheKey(Entity entity, Type returnType, EntityRelation[]? relations, List<NpgsqlDbColumn> columnSchema, bool isChangeTracking)
        {
            _entity = entity;
            _returnType = returnType;
            _relations = relations;
            _columnSchema = columnSchema;
            _isChangeTracking = isChangeTracking;
        }

        public bool Equals(
#if !NET48
            [AllowNull]
#endif
            QueryCacheKey y)
        {
            if (y._isChangeTracking != _isChangeTracking ||
                y._columnSchema.Count != _columnSchema.Count ||
                y._returnType != _returnType)
                return false;

            var columnSchemaSpan = _columnSchema.AsSpan();
            var foreignColumnSchemaSpan = y._columnSchema.AsSpan();

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

            hashCode.Add(_returnType);

            if (_relations is { })
            {
                var columnSchemaSpan = _columnSchema.AsSpan();

                var joinIndex = 0;

                var flattenedPathSpan = _relations.AsSpan();

                Entity nextJoin = _entity;
                string? nextJoinPKName = _entity.GetPrimaryColumn().ColumnName;

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

            hashCode.Add(_isChangeTracking);

            return hashCode.ToHashCode();
        }
    }
}