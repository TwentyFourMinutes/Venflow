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
    }
}
