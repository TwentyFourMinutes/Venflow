using System.Data;
using System.Data.Common;
using System.Text;
using Npgsql;
using Reflow.Lambdas;

namespace Reflow.Commands
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
                    CommandBehavior.SingleRow,
                    cancellationToken
                );

                if (!dataReader.HasRows)
                    return default;

                var columnIndecies = queryData.LambdaData.ColumnIndecies;

                if (columnIndecies is null)
                {
                    var columnSchema = dataReader.GetColumnSchema();

                    queryData.LambdaData.ColumnIndecies = columnIndecies = new ushort[
                        columnSchema.Count
                    ];

                    var entityIndex = 0;
                    var entities = queryData.LambdaData!.UsedEntities;

                    var nextEntity = Entities.Data[entities[entityIndex++]!];
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
                                nextEntity = Entities.Data[entities[entityIndex++]!];
                                nextKeyName = nextEntity.Columns.First().Key;
                            }

                            absoluteIndex += (ushort)nextEntity.Columns.Count;
                            columnIndecies[columnIndex] = (ushort)absoluteIndex;
                        }
                        else if (
                            currentEntity is not null
                            && currentEntity.Columns.TryGetValue(
                                column.ColumnName,
                                out var columnData
                            )
                        )
                        {
                            columnIndecies[columnIndex] = (ushort)(
                                absoluteIndex + columnData.Index
                            );
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }

                await dataReader.ReadAsync();

                return ((Func<DbDataReader, ushort[], TEntity>)queryData.LambdaData.Parser).Invoke(
                    dataReader,
                    columnIndecies
                );
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

        internal static void Handle(IDatabase database, Func<SqlInterpolationHandler> sql)
        {
            var lambdaData = LambdaLinker.GetLambdaData<QueryLinkData>(sql.Method);

            var command = new NpgsqlCommand();

            var commandBuilder = new StringBuilder(lambdaData.MinimumSqlLength);

            var interpolationData = new SqlInterpolationHandler.AmbientData
            {
                ParameterIndecies = lambdaData.ParameterIndecies,
                Parameters = command.Parameters,
                CommandBuilder = commandBuilder,
            };

            SqlInterpolationHandler.AmbientData.Current = interpolationData;

            sql.Invoke();

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
    }
}
