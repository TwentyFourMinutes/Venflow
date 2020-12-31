using System;
using System.Collections;
using System.Text;
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

            for (int spanIndex = 0; spanIndex < sqlLength; spanIndex++)
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

                            for (int listIndex = 0; listIndex < list.Count; listIndex++)
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
    }
}
