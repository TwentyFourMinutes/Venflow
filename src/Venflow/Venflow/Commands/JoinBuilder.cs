using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Venflow.Enums;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;

namespace Venflow.Commands
{
    public class JoinBuilder<TRelationEntity, TEntity, TReturn> where TRelationEntity : class, new() where TEntity : class, new() where TReturn : class, new()
    {
        private readonly TrioKeyCollection<uint, string, EntityRelation>? _relations;
        private readonly JoinBuilderValues _joinBuilderValues;
        private readonly VenflowQueryCommandBuilder<TRelationEntity, TReturn> _commandBuilder;
        private readonly Entity _lastEntity;

        internal JoinBuilder(Entity root, VenflowQueryCommandBuilder<TRelationEntity, TReturn> commandBuilder, bool generateSql)
        {
            _joinBuilderValues = new JoinBuilderValues(root, generateSql);

            _relations = root.Relations;
            _lastEntity = root;
            _commandBuilder = commandBuilder;
        }

        internal JoinBuilder(JoinOptions joinOptions, Entity lastEntity, JoinBuilderValues joinBuilderValues, VenflowQueryCommandBuilder<TRelationEntity, TReturn> commandBuilder, bool newFullPath)
        {
            _relations = joinOptions.Join.RightEntity.Relations;

            joinBuilderValues.AddToPath(joinOptions, newFullPath);

            _joinBuilderValues = joinBuilderValues;
            _lastEntity = lastEntity;
            _commandBuilder = commandBuilder;
        }

        public JoinBuilder<TRelationEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector();

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, true);
        }

        public JoinBuilder<TRelationEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, true);
        }

        public JoinBuilder<TRelationEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, true);
        }

        public JoinBuilder<TRelationEntity, TToEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            propertySelector.ValidatePropertySelector();

            var foreignProperty = propertySelector.ValidatePropertySelector();

            if (!_relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, false);
        }

        public JoinBuilder<TRelationEntity, TToEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!_relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, false);
        }

        public JoinBuilder<TRelationEntity, TToEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!_relations!.TryGetValue(foreignProperty.Name, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity, TReturn>(new JoinOptions(joiningEntity!, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, false);
        }

        public IQueryCommand<TRelationEntity, TReturn> Build()
        {
            _commandBuilder.JoinBuilderValues = _joinBuilderValues;

            return _commandBuilder.Build();
        }

        public IQueryCommandBuilder<TRelationEntity, TReturn> TrackChanges(bool trackChanges = true)
        {
            _commandBuilder.JoinBuilderValues = _joinBuilderValues;

            return _commandBuilder.TrackChanges(trackChanges);
        }
    }
}