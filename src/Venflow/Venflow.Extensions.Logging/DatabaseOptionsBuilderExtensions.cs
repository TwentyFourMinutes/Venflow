using Microsoft.Extensions.Logging;

namespace Venflow.Extensions.Logging
{
    /// <summary>
    /// Providing extensions method for the <see cref=" DatabaseOptionsBuilder{TDatabase}"/> class.
    /// </summary>
    public static class DatabaseOptionsBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="ILoggerFactory"/>, which allows for logging of executed commands.
        /// </summary>
        /// <param name="options">The options to which the <see cref="ILoggerFactory"/> should be registered.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> which should be registered</param>
        /// <param name="formatSensitiveInformation">Determines whether or not the formatter should include the parameters values.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>Also consider configuring the <see cref="DatabaseOptionsBuilder.DefaultLoggingBehavior"/> property.</remarks>
        public static DatabaseOptionsBuilder UseLoggerFactory<TDatabase>(this DatabaseOptionsBuilder<TDatabase> options, ILoggerFactory loggerFactory, bool formatSensitiveInformation = false)
            where TDatabase : Database
        {
            var logger = loggerFactory.CreateLogger(typeof(Database));

            options.LogTo((command, commandType, exception) => logger.Log(exception is null ? LogLevel.Debug : LogLevel.Error, new EventId((int)commandType, null), command, exception,
                (state, exception) =>
                {
                    string commandText;

                    if (state is null)
                    {
                        commandText = "The command was null, no information available."
                    }
                    else
                    {
                        commandText = formatSensitiveInformation ? state.GetUnParameterizedCommandText() : state.CommandText;
                    }

                    return "CommandText: '" + commandText + (exception is not null ? "' Exception: " + exception.Message : "'");
                }));

            return options;
        }
    }
}
