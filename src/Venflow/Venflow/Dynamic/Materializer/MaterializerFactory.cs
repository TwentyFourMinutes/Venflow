using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Npgsql.Schema;
using Venflow.Commands;
using Venflow.Modeling;

namespace Venflow.Dynamic.Materializer
{
    internal class MaterializerFactory<TEntity> where TEntity : class, new()
    {
        private readonly Entity<TEntity> _entity;
        private readonly Dictionary<QueryCacheKey, Delegate> _materializerCache;
        private readonly object _materializerLock;

        internal MaterializerFactory(Entity<TEntity> entity)
        {
            _entity = entity;

            _materializerCache = new(QueryCacheKeyComparer.Default);
            _materializerLock = new();
        }

        internal Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> GetOrCreateMaterializer<TReturn>(RelationBuilderValues? relationBuilderValues, ReadOnlyCollection<NpgsqlDbColumn> columnSchema, bool changeTracking) where TReturn : class, new()
        {
            var cacheKey = new QueryCacheKey(_entity, typeof(TReturn), relationBuilderValues?.GetFlattenedRelations(), columnSchema.AsList(), changeTracking);

            lock (_materializerLock)
            {
                if (_materializerCache.TryGetValue(cacheKey, out var tempMaterializer))
                {
                    return (Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>)tempMaterializer;
                }
                else
                {
                    QueryEntityHolder[] generatedEntities;

                    if (relationBuilderValues is { })
                    {
                        var sourceCompiler = new MaterializerSourceCompiler(relationBuilderValues);

                        sourceCompiler.Compile(_entity);

                        generatedEntities = sourceCompiler.GenerateSortedEntities();
                    }
                    else
                    {
                        generatedEntities = new[] { new QueryEntityHolder(_entity, 0) };
                    }

                    var columnSchemaSpan = columnSchema.AsSpan();

                    var entities = new List<(QueryEntityHolder, List<(EntityColumn, int)>)>();
                    List<(EntityColumn, int)> columns = default;

                    var joinIndex = 1;

                    QueryEntityHolder nextJoin = generatedEntities[0];
                    QueryEntityHolder currentJoin = generatedEntities[0];

                    var nextJoinPKName = _entity.PrimaryColumn?.ColumnName ?? _entity.Columns[0].ColumnName;

                    for (int columnIndex = 0; columnIndex < columnSchemaSpan.Length ; columnIndex++)
                    {
                        var column = columnSchemaSpan[columnIndex];

                        if (column.ColumnName == nextJoinPKName)
                        {
                            if (columnIndex > 0)
                                currentJoin = nextJoin;

                            columns = new List<(EntityColumn, int)>();

                            entities.Add((nextJoin, columns));

                            if (relationBuilderValues is { }
                                && joinIndex < generatedEntities.Length)
                            {
                                nextJoin = generatedEntities[joinIndex];
                                nextJoinPKName = nextJoin.Entity.GetPrimaryColumn().ColumnName;

                                var currentJoinColumnCount = currentJoin.Entity.GetColumnCount();

                                for (int i = currentJoin.Entity.GetRegularColumnOffset(); i < currentJoinColumnCount; i++)
                                {
                                    var currentJoinColumn = currentJoin.Entity.GetColumn(i);

                                    if (currentJoinColumn.ColumnName == nextJoinPKName)
                                    {
                                        throw new InvalidOperationException($"The entity '{currentJoin.Entity.EntityName}' defines the column '{currentJoinColumn.ColumnName}' which can't have the same name, as the joining entity's '{nextJoin.Entity.EntityName}' primary key '{nextJoinPKName}'.");
                                    }
                                }

                                joinIndex++;
                            }
                        }

                        if (!currentJoin.Entity.TryGetColumn(column.ColumnName, out var entityColumn))
                        {
                            throw new InvalidOperationException($"The column '{column.ColumnName}' on entity '{currentJoin.Entity.EntityName}' does not exist.");
                        }

                        columns.Add((entityColumn, column.ColumnOrdinal.Value));
                    }

                    if (relationBuilderValues is { })
                    {
                        if (relationBuilderValues.FlattenedPath.Count + 1 > entities.Count)
                        {
                            throw new InvalidOperationException("You configured more joins than entities returned by the query.");
                        }
                        else if (relationBuilderValues.FlattenedPath.Count + 1 < entities.Count)
                        {
                            throw new InvalidOperationException("You configured fewer joins than entities returned by the query.");
                        }
                    }
                    else if (entities.Count > 1)
                    {
                        throw new InvalidOperationException("The result set contained multiple tables, however the query was configured to only expect one. Try specifying the tables you are joining with JoinWith, while declaring the query.");
                    }

                    var materializer = new MaterializerFactoryCompiler(_entity).CreateMaterializer<TReturn>(entities, changeTracking);

#if NET48
                    _materializerCache.Add(cacheKey, materializer);
#else
                    _materializerCache.TryAdd(cacheKey, materializer);
#endif

                    return materializer;
                }
            }
        }
    }
}