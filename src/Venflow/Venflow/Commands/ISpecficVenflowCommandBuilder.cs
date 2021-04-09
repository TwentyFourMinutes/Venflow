using System;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a generic command builder for all CRUD operations to finalize the configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command which is being configured.</typeparam>
    /// <typeparam name="TLogResult">The type of the command which is being configured after configuring the loggers.</typeparam>
    public interface ISpecficVenflowCommandBuilder<out TCommand, out TLogResult>
        where TCommand : class
        where TLogResult : class
    {
        /// <summary>
        /// Finalizes the ongoing configuration process and builds the command.
        /// </summary>
        /// <returns>The built command.</returns>
        TCommand Build();

        /// <summary>
        /// Determines whether or not to log the command to the provided loggers.
        /// </summary>
        /// <param name="shouldLog">Determines if this command should be logged. This is helpful, if you configured the default logging behavior to be <see langword="true"/>.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You can configure the loggers in the <see cref="Database.Configure(DatabaseOptionsBuilder)"/> method with the <see cref="DatabaseOptionsBuilder.LogTo(Action{string}, bool)"/> methods.</remarks>
        TLogResult Log(bool shouldLog = true);

        /// <summary>
        /// Logs the command to the provided <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The logger which is being used for this command.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>Be aware, that once you configure a logger on a command, the global configured loggers won't be executed for this command.</remarks>
        TLogResult LogTo(LoggerCallback logger);

        /// <summary>
        /// Logs the command to the provided <paramref name="loggers"/>.
        /// </summary>
        /// <param name="loggers">The loggers which are being used for this command.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>Be aware, that once you configure one or more loggers on a command, the global configured loggers won't be executed for this command.</remarks>
        TLogResult LogTo(params LoggerCallback[] loggers);
    }
}