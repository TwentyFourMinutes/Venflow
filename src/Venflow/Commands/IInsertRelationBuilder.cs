using System.Linq.Expressions;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a insert relation builder to configure the insert.
    /// </summary>
    /// <typeparam name="TRelationEntity">The type of the entity which will be inserted with.</typeparam>
    /// <typeparam name="TRootEntity">The root type of the entity.</typeparam>
    public interface IInsertRelationBuilder<TRelationEntity, TRootEntity> : IBaseInsertRelationBuilder<TRelationEntity, TRootEntity>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
    {
        /// <summary>
        /// Allows to configure the inserted relations with the current insert.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the inserted entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get inserted with the root entity during insertion.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure the inserted relations with the current insert.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the inserted entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get inserted with the root entity during insertion.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure the inserted relations with the current insert.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the inserted entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get inserted with the root entity during insertion.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();
    }
}
