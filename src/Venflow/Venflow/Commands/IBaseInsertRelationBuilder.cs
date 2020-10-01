using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a base insert relation builder to configure the insert.
    /// </summary>
    /// <typeparam name="TRelationEntity">The type of the entity which will be joined with.</typeparam>
    /// <typeparam name="TRootEntity">The root type of the entity.</typeparam>
    public interface IBaseInsertRelationBuilder<TRelationEntity, TRootEntity> : IInsertCommandBuilder<TRootEntity>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
    {
        /// <summary>
        /// Allows to configure the current insert, to insert all populated relations which can be reached.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IBaseInsertRelationBuilder<TRootEntity, TRootEntity> InsertWithAll();

        /// <summary>
        /// Allows to configure the inserted relations with the current insert.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the inserted entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get inserted with the root entity during insertion.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure the inserted relations with the current insert.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the inserted entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get inserted with the root entity during insertion.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure the inserted relations with the current insert.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the inserted entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get inserted with the root entity during insertion.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();
    }
}
