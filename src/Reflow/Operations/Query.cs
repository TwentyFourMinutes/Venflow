using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using Reflow.Lambdas;

namespace Reflow.Operations
{
    internal static class Query
    {
        private class AmbientData
        {
            [ThreadStatic]
            internal static AmbientData? Current;

            internal QueryLinkData LambdaData = null!;
            internal DbCommand Command = null!;
            internal IDatabase Database = null!;
            internal MethodInfo MethodInfo = null!;

            internal Action SqlInterpolationHandler = null!;
            internal IInterpolationArgument[] Arguments = null!;
        }

        internal static ValueTask<TEntity?> SingleAsync<TEntity>(
            bool hasRelations,
            CancellationToken cancellationToken
        )
        {
            var queryData = AmbientData.Current ?? throw new InvalidOperationException();

            if (queryData.LambdaData.Caching)
            {
                return GetCachedAsync(hasRelations, SingleBaseAsync<TEntity>, cancellationToken);
            }
            else
            {
                return SingleBaseAsync<TEntity>(hasRelations, cancellationToken);
            }
        }

        private static async ValueTask<TEntity?> SingleBaseAsync<TEntity>(
            bool hasRelations,
            CancellationToken cancellationToken
        )
        {
            SqlInterpolationHandler.AmbientData.Current = null;

            var queryData = AmbientData.Current ?? throw new InvalidOperationException();
            AmbientData.Current = null;

            await queryData.Database.EnsureValidConnection(cancellationToken);
            queryData.Command.Connection = queryData.Database.Connection;

            DbDataReader dataReader = null!;

            try
            {
                var commandBehaviour = hasRelations
                    ? CommandBehavior.SingleResult | CommandBehavior.SequentialAccess
                    : CommandBehavior.SingleRow
                      | CommandBehavior.SingleResult
                      | CommandBehavior.SequentialAccess;

                dataReader = await queryData.Command.ExecuteReaderAsync(
                    commandBehaviour,
                    cancellationToken
                );

                if (!dataReader.HasRows)
                    return default;

                var columnIndecies = queryData.LambdaData.ColumnIndecies;

                if (columnIndecies is null)
                {
                    GetColumnIndecies(
                        queryData.Database,
                        dataReader.GetColumnSchema(),
                        queryData.LambdaData!.UsedEntities,
                        out columnIndecies
                    );

                    queryData.LambdaData.ColumnIndecies = columnIndecies;
                }

                if (hasRelations)
                {
                    return await (
                        (Func<DbDataReader, ushort[], Task<TEntity>>)queryData.LambdaData.Parser
                    ).Invoke(dataReader, columnIndecies);
                }
                else
                {
                    await dataReader.ReadAsync();

                    return (
                        (Func<DbDataReader, ushort[], TEntity>)queryData.LambdaData.Parser
                    ).Invoke(dataReader, columnIndecies);
                }
            }
            catch
            {
                return default;
            }
            finally
            {
                if (dataReader is not null)
                    await dataReader.DisposeAsync();

                await queryData.Command.DisposeAsync();
            }
        }

        internal static ValueTask<IList<TEntity>> ManyAsync<TEntity>(
            CancellationToken cancellationToken
        )
        {
            var queryData = AmbientData.Current ?? throw new InvalidOperationException();

            if (queryData.LambdaData.Caching)
            {
                return GetCachedAsync(false, ManyBaseAsync<TEntity>, cancellationToken);
            }
            else
            {
                return ManyBaseAsync<TEntity>(false, cancellationToken);
            }
        }

        private static async ValueTask<IList<TEntity>> ManyBaseAsync<TEntity>(
            bool ignore,
            CancellationToken cancellationToken
        )
        {
            _ = ignore;
            var queryData = AmbientData.Current!;

            await queryData.Database.EnsureValidConnection(cancellationToken);
            queryData.Command.Connection = queryData.Database.Connection;

            DbDataReader dataReader = null!;

            try
            {
                dataReader = await queryData.Command.ExecuteReaderAsync(
                    CommandBehavior.SingleResult | CommandBehavior.SequentialAccess,
                    cancellationToken
                );

                if (!dataReader.HasRows)
                    return Array.Empty<TEntity>();

                var columnIndecies = queryData.LambdaData.ColumnIndecies;

                if (columnIndecies is null)
                {
                    GetColumnIndecies(
                        queryData.Database,
                        dataReader.GetColumnSchema(),
                        queryData.LambdaData!.UsedEntities,
                        out columnIndecies
                    );
                    queryData.LambdaData.ColumnIndecies = columnIndecies;
                }

                return await (
                    (Func<DbDataReader, ushort[], Task<IList<TEntity>>>)queryData.LambdaData.Parser
                ).Invoke(dataReader, columnIndecies);
            }
            catch
            {
                return Array.Empty<TEntity>();
            }
            finally
            {
                if (dataReader is not null)
                    await dataReader.DisposeAsync();

                await queryData.Command.DisposeAsync();

                AmbientData.Current = null;
            }
        }

        private static ValueTask<TReturn> GetCachedAsync<TReturn>(
            bool hasRelations,
            Func<bool, CancellationToken, ValueTask<TReturn>> queryFunc,
            CancellationToken cancellationToken
        )
        {
            var interpolationData = SqlInterpolationHandler.AmbientData.Current;
            var queryData = AmbientData.Current!;

            return queryData.Database.QueryCache.GetOrCreate(
                new QueryCacheKey(queryData.MethodInfo, queryData.Arguments),
                _ =>
                {
                    queryData.SqlInterpolationHandler.Invoke();

                    return queryFunc.Invoke(hasRelations, cancellationToken);
                }
            );
        }

        internal static void Handle<TDelegate>(
            IDatabase database,
            TDelegate data,
            Action<TDelegate> sql
        ) where TDelegate : Delegate
        {
            var lambdaData = database.GetQueryData<QueryLinkData>(data.Method);

            if (lambdaData.Caching && lambdaData.ParameterIndecies is null)
            {
                AmbientData.Current = new AmbientData
                {
                    Database = database,
                    LambdaData = lambdaData,
                    MethodInfo = sql.Method,
                    SqlInterpolationHandler = () =>
                    {
                        var commandBuilder = new StringBuilder(lambdaData.MinimumSqlLength);

                        SqlInterpolationHandler.AmbientData.Current =
                            new SqlInterpolationHandler.AmbientData
                            {
                                HelperStrings = lambdaData.HelperStrings!,
                                CommandBuilder = commandBuilder!,
                                Caching = true,
                            };

                        sql.Invoke(data);

                        SqlInterpolationHandler.AmbientData.Current = null;

                        AmbientData.Current!.Command = new NpgsqlCommand(commandBuilder.ToString());
                    }
                };
            }
            else
            {
                var command = new NpgsqlCommand();

                var commandBuilder = new StringBuilder(lambdaData.MinimumSqlLength);

                var interpolationData = new SqlInterpolationHandler.AmbientData
                {
                    ParameterIndecies = lambdaData.ParameterIndecies!,
                    HelperStrings = lambdaData.HelperStrings!,
                    Parameters = command.Parameters,
                    CommandBuilder = commandBuilder!,
                    Caching = lambdaData.Caching,
                };

                SqlInterpolationHandler.AmbientData.Current = interpolationData;

                sql.Invoke(data);

                var queryData = new AmbientData
                {
                    Command = command,
                    LambdaData = lambdaData,
                    Database = database,
                    MethodInfo = data.Method,
                };

                if (lambdaData.Caching)
                {
                    queryData.SqlInterpolationHandler = () =>
                    {
                        interpolationData.Caching = false;
                        interpolationData.Arguments = null!;
                        SqlInterpolationHandler.AmbientData.Current = interpolationData;

                        sql.Invoke(data);

                        SqlInterpolationHandler.AmbientData.Current = null;

                        queryData.Command.CommandText = interpolationData.CommandBuilder.ToString();
                    };
                    queryData.Arguments = interpolationData.Arguments;
                }
                else
                {
                    command.CommandText = commandBuilder!.ToString();
                }

                AmbientData.Current = queryData;
            }
        }

        internal static void HandleRaw(IDatabase database, Func<string> sql)
        {
            var queryData = database.GetQueryData<QueryLinkData>(sql.Method);

            var ambientData = new AmbientData
            {
                LambdaData = queryData,
                Database = database,
                MethodInfo = sql.Method
            };

            if (queryData.Caching)
            {
                ambientData.SqlInterpolationHandler = () =>
                    AmbientData.Current!.Command = new NpgsqlCommand(sql.Invoke());
            }
            else
            {
                ambientData.Command = new NpgsqlCommand(sql.Invoke());
            }

            AmbientData.Current = ambientData;
        }

        private static void GetColumnIndecies(
            IDatabase database,
            ReadOnlyCollection<DbColumn> columnSchema,
            Type[] entities,
            out ushort[] columnIndecies
        )
        {
            columnIndecies = new ushort[columnSchema.Count];

            var entityIndex = 0;

            var nextEntity = database.Configuration.Entities[entities[entityIndex++]!];
            Entity? currentEntity = null;
            var nextKeyName = nextEntity.Columns.First().Key;
            var absoluteIndex = (ushort)nextEntity.Columns.Count * -1;

            for (var columnIndex = 0; columnIndex < columnSchema.Count; columnIndex++)
            {
                var column = columnSchema[columnIndex];

                if (column.ColumnName == nextKeyName)
                {
                    currentEntity = nextEntity;

                    if (entityIndex < entities.Length)
                    {
                        nextEntity = database.Configuration.Entities[entities[entityIndex++]!];
                        nextKeyName = nextEntity.Columns.First().Key;
                    }

                    absoluteIndex += (ushort)nextEntity.Columns.Count;
                    columnIndecies[columnIndex] = (ushort)absoluteIndex;
                }
                else if (
                    currentEntity is not null
                    && currentEntity.Columns.TryGetValue(column.ColumnName, out var columnData)
                )
                {
                    columnIndecies[columnIndex] = (ushort)(absoluteIndex + columnData.Index);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
