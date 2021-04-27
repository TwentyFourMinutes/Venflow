using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using Venflow.Enums;

namespace Venflow
{
    /// <summary>
    /// Represent a method that will handle all Logs produced by a <see cref="Database"/> instance.
    /// </summary>
    /// <param name="command">The command which produced the log.</param>
    /// <param name="commandType">The command type which produced the log.</param>
    /// <param name="exception">The exception occurred while trying to execute the command, if any occurred.</param>
    public delegate void LoggerCallback(NpgsqlCommand command, CommandType commandType, Exception? exception);

    /// <summary>
    /// Provides an option builder to further <i>dynamically</i> configure a <see cref="Database"/> instance.
    /// </summary>
    public class DatabaseOptionsBuilder<TDatabase> : DatabaseOptionsBuilder
        where TDatabase : Database
    {
        /// <summary>
        /// Adds a logger, which allows for logging of executed commands.
        /// </summary>
        /// <param name="loggerCallback">A callback which is being used to log commands.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>
        /// Also consider configuring the <see cref="DatabaseOptionsBuilder.DefaultLoggingBehavior"/> property.
        /// </remarks>
        public DatabaseOptionsBuilder<TDatabase> LogTo(LoggerCallback loggerCallback)
        {
            Loggers.Add(loggerCallback);

            return this;
        }

        /// <summary>
        /// Adds a logger, which allows for logging of executed commands.
        /// </summary>
        /// <param name="loggerCallback">A callback which is being used to log commands.</param>
        /// <param name="logSensitveData">Determines whether or not to log parameterized commands.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>
        /// Also consider configuring the <see cref="DatabaseOptionsBuilder.DefaultLoggingBehavior"/> property.
        /// Be aware that this method should be used in cases which require quick logging. This API wraps the <paramref name="loggerCallback"/> again and calls <see cref="LogTo(LoggerCallback)"/>.
        /// </remarks>
        public DatabaseOptionsBuilder<TDatabase> LogTo(Action<string> loggerCallback, bool logSensitveData = false)
        {
            Loggers.Add((command, type, exception) =>
            {
                var sb = new StringBuilder();

                sb.Append("Type: ")
                  .Append(type)
                  .Append(": ");

                if (logSensitveData)
                {
                    sb.AppendLine(command.GetUnParameterizedCommandText());
                }
                else
                {
                    sb.AppendLine(command.CommandText);
                }

                if (exception is not null)
                {
                    sb.Append("Exception: ")
                      .Append(exception);
                }

                loggerCallback.Invoke(sb.ToString());
            });

            return this;
        }
    }

    /// <summary>
    /// Represent a method that will handle all Logs produced by a <see cref="Database"/> instance.
    /// </summary>
    public abstract class DatabaseOptionsBuilder
    {
        /// <summary>
        /// Gets or sets the default LoggingBehavior on commands for this <see cref="Database"/>. The default is <see cref="LoggingBehavior.Always"/>, if any loggers are defined.
        /// </summary>
        public LoggingBehavior DefaultLoggingBehavior { get; set; }

        /// <summary>
        /// Gets or sets the connection string which will be used in all <see cref="Database"/> instances using the current <see cref="DatabaseOptionsBuilder{TDatabase}"/> instance.
        /// </summary>
        public string ConnectionString { get; set; }

        internal List<LoggerCallback> Loggers { get; }

        private protected DatabaseOptionsBuilder()
        {
            Loggers = new();
        }
    }
}
