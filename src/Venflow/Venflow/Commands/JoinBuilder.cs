using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Venflow.Enums;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;
using Venflow.Models;

namespace Venflow.Commands
{
    public class JoinBuilder<TRelationEntity, TEntity> where TRelationEntity : class where TEntity : class
    {
        private readonly DualKeyCollection<string, EntityRelation>? _relations;
        private readonly JoinBuilderValues _joinBuilderValues;
        private readonly VenflowCommandBuilder<TRelationEntity> _commandBuilder;
        private readonly Entity _lastEntity;

        internal JoinBuilder(Entity root, VenflowCommandBuilder<TRelationEntity> commandBuilder)
        {
            _joinBuilderValues = new JoinBuilderValues(root);

            _relations = root.Relations;
            _lastEntity = root;
            _commandBuilder = commandBuilder;
        }

        internal JoinBuilder(JoinOptions joinOptions, Entity lastEntity, JoinBuilderValues joinBuilderValues, VenflowCommandBuilder<TRelationEntity> commandBuilder, bool newFullPath)
        {
            _relations = joinOptions.JoinWith.RightEntity.Relations;

            joinBuilderValues.AddToPath(joinOptions, newFullPath);

            _joinBuilderValues = joinBuilderValues;
            _lastEntity = lastEntity;
            _commandBuilder = commandBuilder;
        }

        public JoinBuilder<TRelationEntity, TToEntity> JoinWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            propertySelector.ValidatePropertySelector();

            var foreignPropertyType = typeof(TEntity);

            var propertyName = foreignPropertyType.IsGenericType ? foreignPropertyType.GetGenericArguments()[0].Name : foreignPropertyType.Name;

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(propertyName, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}'.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity>(new JoinOptions(joiningEntity!, _lastEntity, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, true);
        }

        public JoinBuilder<TRelationEntity, TToEntity> JoinWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            propertySelector.ValidatePropertySelector();

            var foreignPropertyType = typeof(TEntity);

            var propertyName = foreignPropertyType.IsGenericType ? foreignPropertyType.GetGenericArguments()[0].Name : foreignPropertyType.Name;

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(propertyName, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}'.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity>(new JoinOptions(joiningEntity!, _lastEntity, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, true);
        }


        public JoinBuilder<TRelationEntity, TToEntity> ThenWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            propertySelector.ValidatePropertySelector();

            var foreignPropertyType = typeof(TToEntity);

            var propertyName = foreignPropertyType.IsGenericType ? foreignPropertyType.GetGenericArguments()[0].Name : foreignPropertyType.Name;

            if (!_relations!.TryGetValue(propertyName, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}'.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity>(new JoinOptions(joiningEntity!, _lastEntity, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, false);
        }

        public JoinBuilder<TRelationEntity, TToEntity> ThenWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            propertySelector.ValidatePropertySelector();

            var foreignPropertyType = typeof(TToEntity);

            var propertyName = foreignPropertyType.IsGenericType ? foreignPropertyType.GetGenericArguments()[0].Name : foreignPropertyType.Name;

            if (!_relations!.TryGetValue(propertyName, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}'.");
            }

            return new JoinBuilder<TRelationEntity, TToEntity>(new JoinOptions(joiningEntity!, _lastEntity, joinBehaviour), joiningEntity!.RightEntity, _joinBuilderValues, _commandBuilder, false);
        }

        public IQueryCommand<TRelationEntity> Single()
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Single();
        }

        public IQueryCommand<TRelationEntity> Single(string sql, params NpgsqlParameter[] parameters)
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Single(sql, parameters);
        }

        public IQueryCommand<TRelationEntity> Batch()
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Batch();
        }

        public IQueryCommand<TRelationEntity> Batch(ulong count)
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Batch(count);
        }

        public IQueryCommand<TRelationEntity> Batch(string sql, params NpgsqlParameter[] parameters)
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Batch(sql, parameters);
        }
    }
}