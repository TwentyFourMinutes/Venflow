using Npgsql;

namespace Venflow
{
    /// <summary>
    /// Provides a set of useful extension methods for the <see cref="NpgsqlCommand"/> class.
    /// </summary>
    public static class NpgsqlCommandExtensions
    {
        /// <summary>
        /// Adds a new Parameter to the <see cref="NpgsqlCommand.Parameters"/> collection.
        /// </summary>
        /// <typeparam name="TType">The value type of the Parameter.</typeparam>
        /// <param name="command">The command to which the Parameter should be added.</param>
        /// <param name="parameterName">The name of the Parameter.</param>
        /// <param name="value">The value of the Parameter.</param>
        /// <returns>the just created <see cref="NpgsqlParameter{TType}"/>.</returns>
        public static NpgsqlParameter<TType> AddParamter<TType>(this NpgsqlCommand command, string parameterName, TType value)
        {
            var parameter = new NpgsqlParameter<TType>(parameterName, value);

            command.Parameters.Add(parameter);

            return parameter;
        }

        /// <summary>
        /// Sets the command text of the used <see cref="NpgsqlCommand"/>. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="command">The command of which the command text should be set. Ensure that you do not pass any user manipulated SQL for this parameter. You should only add parameters trough string interpolation.</param>
        /// <param name="sql">A string containing the SQL statement.</param>
        public static void SetInterpolatedCommandText(this NpgsqlCommand command, FormattableString sql)
        {
            var argumentsSpan = sql.GetArguments().AsSpan();

            var sqlLength = sql.Format.Length;

            var argumentedSql = new StringBuilder(sqlLength);

            var sqlSpan = sql.Format.AsSpan();

            var argumentIndex = 0;
            var parameterIndex = 0;

            for (var spanIndex = 0; spanIndex < sqlLength; spanIndex++)
            {
                var spanChar = sqlSpan[spanIndex];

                if (spanChar == '{' &&
                    spanIndex + 2 < sqlLength)
                {
                    for (spanIndex++; spanIndex < sqlLength; spanIndex++)
                    {
                        spanChar = sqlSpan[spanIndex];

                        if (spanChar == '}')
                            break;

                        if (spanChar is < '0' or > '9')
                            throw new InvalidOperationException();
                    }

                    var argument = argumentsSpan[argumentIndex++];

                    if (argument is IList list)
                    {
                        if (list.Count > 0)
                        {
                            var listType = default(Type);

                            for (var listIndex = 0; listIndex < list.Count; listIndex++)
                            {
                                var listItem = list[listIndex];

                                if (listType is null &&
                                    listItem is not null)
                                {
                                    listType = listItem.GetType();

                                    if (listType == typeof(object))
                                        throw new InvalidOperationException("The SQL string interpolation doesn't support object lists.");
                                }

                                var parameterName = "@p" + parameterIndex++.ToString();

                                argumentedSql.Append(parameterName)
                                             .Append(", ");

                                command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, listType!, listItem));
                            }

                            argumentedSql.Length -= 2;
                        }

                        parameterIndex--;
                    }
                    else
                    {
                        var parameterName = "@p" + parameterIndex++.ToString();

                        argumentedSql.Append(parameterName);

                        command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, argument));
                    }
                }
                else
                {
                    argumentedSql.Append(spanChar);
                }
            }

            command.CommandText = argumentedSql.ToString();
        }

        /// <summary>
        /// Gets the un-parameterized command text of the used <see cref="NpgsqlCommand"/>.
        /// </summary>
        /// <param name="command">The command of which the command text should be set. Ensure that you do not pass any user manipulated SQL for this parameter. You should only add parameters trough string interpolation.</param>
        /// <returns>The un-parameterized command text.</returns>
        /// <remarks>
        /// <strong>This API may very likely not return the exact SQL the server will be using!</strong> This is due to its client-side implementation. The returned SQL is just a rough estimate of what the server may be using. Additionally this API does require the naming of parameters to be like '@p' followed by their index. Furthermore the parameters have to be in the exact order as their placeholder.
        /// </remarks>
        public static string GetUnParameterizedCommandText(this NpgsqlCommand command)
        {
            var sqlLength = command.CommandText.Length;

            var argumentedSql = new StringBuilder(sqlLength);

            var sqlSpan = command.CommandText.AsSpan();

            var argumentIndex = 0;

            for (var spanIndex = 0; spanIndex < sqlLength; spanIndex++)
            {
                var spanChar = sqlSpan[spanIndex];

                if (spanChar == '@' &&
                    spanIndex + 2 < sqlLength &&
                    sqlSpan[spanIndex + 1] == 'p')
                {
                    for (spanIndex++; spanIndex < sqlLength; spanIndex++)
                    {
                        spanChar = sqlSpan[spanIndex];

                        if (spanChar is < '0' or > '9')
                            break;
                    }

                    spanIndex++;

                    var argument = command.Parameters[argumentIndex++];

                    if (argument.Value is null or DBNull)
                    {
                        argumentedSql.Append("null");
                    }
                    else
                    {
                        argumentedSql.Append('\'')
                                     .Append(argument.Value.ToString())
                                     .Append('\'');
                    }
                }
                else
                {
                    argumentedSql.Append(spanChar);
                }
            }

            return argumentedSql.ToString();
        }
    }
}
