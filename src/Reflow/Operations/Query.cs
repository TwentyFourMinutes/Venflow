using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Text;
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
        }

        internal static async Task<TEntity?> SingleAsync<TEntity>(
            bool hasRelations,
            CancellationToken cancellationToken
        )
        {
            var queryData = AmbientData.Current ?? throw new InvalidOperationException();

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

        internal static async Task<IList<TEntity>> ManyAsync<TEntity>(
            CancellationToken cancellationToken
        )
        {
            var queryData = AmbientData.Current ?? throw new InvalidOperationException();

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
            }
        }

        internal static void Handle<TDelegate>(
            IDatabase database,
            TDelegate data,
            Action<TDelegate> sql
        ) where TDelegate : Delegate
        {
            var lambdaData = database.GetQueryData<QueryLinkData>(data.Method);

            var command = new NpgsqlCommand();

            var commandBuilder = new StringBuilder(lambdaData.MinimumSqlLength);

            var interpolationData = new SqlInterpolationHandler.AmbientData
            {
                ParameterIndecies = lambdaData.ParameterIndecies!,
                HelperStrings = lambdaData.HelperStrings!,
                Parameters = command.Parameters,
                CommandBuilder = commandBuilder,
            };

            SqlInterpolationHandler.AmbientData.Current = interpolationData;

            sql.Invoke(data);

            SqlInterpolationHandler.AmbientData.Current = null;

            command.CommandText = commandBuilder.ToString();

            var queryData = new AmbientData
            {
                Command = command,
                LambdaData = lambdaData,
                Database = database
            };

            AmbientData.Current = queryData;
        }

        internal static void HandleRaw(IDatabase database, Func<string> sql)
        {
            var queryData = new AmbientData
            {
                Command = new NpgsqlCommand(sql.Invoke()),
                LambdaData = database.GetQueryData<QueryLinkData>(sql.Method),
                Database = database
            };

            AmbientData.Current = queryData;
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
