using Npgsql;
using Npgsql.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Commands;
using Venflow.Modeling;

namespace Venflow.Dynamic.Materializer
{
    internal class MaterializerFactory<TEntity> where TEntity : class, new()
    {
        private readonly Entity<TEntity> _entity;
        private readonly Dictionary<int, Delegate> _materializerCache;
        private readonly object _materializerLock;

        internal MaterializerFactory(Entity<TEntity> entity)
        {
            _entity = entity;

            _materializerCache = new Dictionary<int, Delegate>();
            _materializerLock = new object();
        }

        internal Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> GetOrCreateMaterializer<TReturn>(JoinBuilderValues? joinBuilderValues, ReadOnlyCollection<NpgsqlDbColumn> columnSchema, bool changeTracking) where TReturn : class, new()
        {
            var cacheKeyBuilder = new HashCode();

            cacheKeyBuilder.Add(typeof(TReturn));

            var hasJoins = joinBuilderValues is { };

            var columnIndex = 0;

            if (hasJoins)
            {
                var joinIndex = 0;

                Entity nextJoin = _entity;
                string? nextJoinPKName = _entity.PrimaryColumn.ColumnName;

                for (; columnIndex < columnSchema.Count; columnIndex++)
                {
                    var columnName = columnSchema[columnIndex].ColumnName;

                    if (columnName == nextJoinPKName)
                    {
                        cacheKeyBuilder.Add(nextJoin.EntityName);

                        if (joinBuilderValues.Joins.Count == joinIndex)
                            break;

                        nextJoin = joinBuilderValues.Joins[joinIndex].Join.RightEntity;
                        nextJoinPKName = nextJoin.GetPrimaryColumn().ColumnName;

                        joinIndex++;
                    }

                    cacheKeyBuilder.Add(columnName);
                }
            }
            else
            {
                cacheKeyBuilder.Add(_entity.TableName);
            }

            for (; columnIndex < columnSchema.Count; columnIndex++)
            {
                cacheKeyBuilder.Add(columnSchema[columnIndex].ColumnName);
            }

            cacheKeyBuilder.Add(changeTracking);

            var cacheKey = cacheKeyBuilder.ToHashCode();

            lock (_materializerLock)
            {
                if (_materializerCache.TryGetValue(cacheKey, out var tempMaterializer))
                {
                    return (Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>)tempMaterializer;
                }
                else
                {
                    QueryEntityHolder[] generatedEntities;

                    if (hasJoins)
                    {
                        var sourceCompiler = new MaterializerSourceCompiler(joinBuilderValues);

                        sourceCompiler.Compile();

                        generatedEntities = sourceCompiler.GenerateSortedEntities();
                    }
                    else
                    {
                        generatedEntities = new[] { new QueryEntityHolder(_entity, 0) };
                    }

                    var entities = new List<(QueryEntityHolder, List<(EntityColumn, int)>)>();
                    List<(EntityColumn, int)> columns = default;

                    var joinIndex = 1;

                    QueryEntityHolder nextJoin = generatedEntities[0];
                    QueryEntityHolder currentJoin = generatedEntities[0];

                    var nextJoinPKName = _entity.PrimaryColumn?.ColumnName ?? _entity.Columns[0].ColumnName;

                    for (columnIndex = 0; columnIndex < columnSchema.Count; columnIndex++)
                    {
                        var column = columnSchema[columnIndex];

                        if (column.ColumnName == nextJoinPKName)
                        {
                            if (columnIndex > 0)
                                currentJoin = nextJoin;

                            columns = new List<(EntityColumn, int)>();

                            entities.Add((nextJoin, columns));

                            if (hasJoins && joinIndex < generatedEntities.Length)
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

                    if (hasJoins)
                    {
                        if (joinBuilderValues.Joins.Count + 1 > entities.Count)
                        {
                            throw new InvalidOperationException("You configured more joins than entities returned by the query.");
                        }
                        else if (joinBuilderValues.Joins.Count + 1 < entities.Count)
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