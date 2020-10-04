using System;
using System.Runtime.CompilerServices;
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

            var parameterNames = new string[argumentsSpan.Length];
            var parameterNamesSpan = parameterNames.AsSpan();

            for (int argumentIndex = 0; argumentIndex < argumentsSpan.Length; argumentIndex++)
            {
                var parameterName = "@p" + argumentIndex;

                parameterNamesSpan[argumentIndex] = parameterName;

                command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, argumentsSpan[argumentIndex]));
            }

            command.CommandText = string.Format(sql.Format, parameterNames);
        }
    }
}
