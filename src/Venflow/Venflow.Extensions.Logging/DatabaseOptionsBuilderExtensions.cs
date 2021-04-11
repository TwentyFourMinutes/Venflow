using Microsoft.Extensions.Logging;

namespace Venflow.Extensions.Logging
{
    public static class DatabaseOptionsBuilderExtensions
    {
        public static DatabaseOptionsBuilder UseLoggerFactory(this DatabaseOptionsBuilder options, ILoggerFactory loggerFactory, bool formatSensitiveInformation = false)
        {
            var logger = loggerFactory.CreateLogger(options.EffectiveDatabaseType);

            options.LogTo((command, commandType, exception) => logger.Log(exception is null ? LogLevel.Debug : LogLevel.Error, new EventId((int)commandType, null), command, exception,
                (state, exception) =>
                {
                    return "CommandText: '" + (formatSensitiveInformation ? state.GetUnParameterizedCommandText() : state.CommandText) + (exception is not null ? "' Exception: " + exception.Message : "'");
                }));

            return options;
        }
    }
}
