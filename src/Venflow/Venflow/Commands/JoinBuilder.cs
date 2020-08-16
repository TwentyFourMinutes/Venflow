using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Venflow.Enums;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a query command builder which helps configuring the materialized joins.
    /// </summary>
    /// <typeparam name="TRelationEntity">The entity which will be joined with.</typeparam>
    /// <typeparam name="TEntity">The root entity of the query.</typeparam>
    /// <typeparam name="TReturn">The return type of the query.</typeparam>
    public class JoinBuilder<TRelationEntity, TEntity, TReturn> where TRelationEntity : class, new() where TEntity : class, new() where TReturn : class, new()
    {
        private readonly TrioKeyCollection<uint, string, EntityRelation>? _relations;
        private readonly JoinBuilderValues _joinBuilderValues;
        private readonly VenflowQueryCommandBuilder<TRelationEntity, TReturn> _commandBuilder;

        internal JoinBuilder(Entity root, VenflowQueryCommandBuilder<TRelationEntity, TReturn> commandBuilder, bool generateSql)
        {
            _joinBuilderValues = new JoinBuilderValues(root, generateSql);

            _relations = root.Relations;
            _commandBuilder = commandBuilder;
        }

        internal JoinBuilder(JoinOptions joinOptions, JoinBuilderValues joinBuilderValues, VenflowQueryCommandBuilder<TRelationEntity, TReturn> commandBuilder, bool newFullPath)
        {
            _relations = joinOptions.Join.RightEntity.Relations;

            joinBuilderValues.AddToPath(joinOptions, newFullPath);

            _joinBuilderValues = joinBuilderValues;
            _commandBuilder = commandBuilder;
        }

        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        public JoinBuilder<TRelationEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector();

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), _joinBuilderValues, _commandBuilder, true);
        }

        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        public JoinBuilder<TRelationEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), _joinBuilderValues, _commandBuilder, true);
        }

        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        public JoinBuilder<TRelationEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), _joinBuilderValues, _commandBuilder, true);
        }

        /// <summary>
        /// Allows to configure concatenated materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        public JoinBuilder<TRelationEntity, TToEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            propertySelector.ValidatePropertySelector();

            var foreignProperty = propertySelector.ValidatePropertySelector();

            if (!_relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), _joinBuilderValues, _commandBuilder, false);
        }

        /// <summary>
        /// Allows to configure concatenated materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        public JoinBuilder<TRelationEntity, TToEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!_relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), _joinBuilderValues, _commandBuilder, false);
        }

        /// <summary>
        /// Allows to configure concatenated materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        public JoinBuilder<TRelationEntity, TToEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!_relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), _joinBuilderValues, _commandBuilder, false);
        }

        /// <summary>
        /// Finalizes the ongoing configuration process and builds the command.
        /// </summary>
        /// <returns>The built command.</returns>
        public IQueryCommand<TRelationEntity, TReturn> Build()
        {
            _commandBuilder.JoinBuilderValues = _joinBuilderValues;

            return _commandBuilder.Build();
        }

        /// <summary>
        /// Determines whether or not to return change tracked entities from the query.
        /// </summary>
        /// <param name="trackChanges">Determines if change tracking should be applied.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        public IQueryCommandBuilder<TRelationEntity, TReturn> TrackChanges(bool trackChanges = true)
        {
            _commandBuilder.JoinBuilderValues = _joinBuilderValues;

            return _commandBuilder.TrackChanges(trackChanges);
        }
    }
}