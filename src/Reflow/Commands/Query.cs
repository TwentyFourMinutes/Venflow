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

            internal LambdaData LambdaData = null!;
            internal DbCommand Command = null!;
        }

        internal static async Task<T?> SingleAsync<T>(CancellationToken cancellationToken)
        {
            var queryData = AmbientData.Current ?? throw new InvalidOperationException();

            var dataReader = await queryData.Command.ExecuteReaderAsync(
                CommandBehavior.SingleRow,
                cancellationToken
            );

            try
            {
                if (!dataReader.HasRows)
                    return default;

                var columnIndecies = queryData.LambdaData.ColumnIndecies;

                if (columnIndecies is null)
                {
                    var columnSchema = dataReader.GetColumnSchema();

                    queryData.LambdaData.ColumnIndecies = columnIndecies = new short[
                        columnSchema.Count
                    ];

                    for (var columnIndex = 0; columnIndex < columnSchema.Count; columnIndex++)
                    {
                        // TODO

                        var column = columnSchema[columnIndex];

                        columnIndecies[columnIndex] =
                            (short)column.ColumnOrdinal.GetValueOrDefault();
                    }
                }

                await dataReader.ReadAsync();
            }
            finally
            {
                await dataReader.DisposeAsync();
                await queryData.Command.DisposeAsync();
            }

            return default;
        }

        internal static void Handle(Func<SqlInterpolationHandler> sql)
        {
            var lambdaData = LambdaLinker.GetLambdaData(sql.Method);

            var command = new NpgsqlCommand();

            var commandBuilder = new StringBuilder(lambdaData.MinimumSqlLength);

            var interpolationData = new SqlInterpolationHandler.AmbientData
            {
                ParameterIndecies = lambdaData.ParameterIndecies,
                Parameters = command.Parameters,
                CommandBuilder = commandBuilder
            };

            SqlInterpolationHandler.AmbientData.Current = interpolationData;

            sql.Invoke();

            SqlInterpolationHandler.AmbientData.Current = null;

            command.CommandText = commandBuilder.ToString();

            var queryData = new AmbientData { Command = command, LambdaData = lambdaData };

            AmbientData.Current = queryData;
        }
    }
}
