namespace Venflow.Commands
{
    /// <summary>
    /// Represents a generic command builder for all CRUD operations to finalize the configuration.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command which is being configured.</typeparam>
    public interface ISpecficVenflowCommandBuilder<out TCommand> where TCommand : class
    {
        /// <summary>
        /// Finalizes the ongoing configuration process and builds the command.
        /// </summary>
        /// <returns>The built command.</returns>
        TCommand Build();
    }
}