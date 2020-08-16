namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command builder to configure the insertion.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be inserted.</typeparam>
    public interface IInsertCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IInsertCommand<TEntity>> where TEntity : class, new()
    {
        /// <summary>
        /// Defines if the insertion should populate identity columns.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertCommandBuilder<TEntity> SetIdentityColumns();
        /// <summary>
        /// Defines if the insertion should populate all foreign properties.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertCommandBuilder<TEntity> PopulateRelation();
        /// <summary>
        /// Defines if the insertion shouldn't populate identity columns.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertCommandBuilder<TEntity> DoNotSetIdentityColumns();
        /// <summary>
        /// Defines if the insertion shouldn't populate all foreign properties.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertCommandBuilder<TEntity> DoNotDoNotSetPopulateRelation();
    }
}