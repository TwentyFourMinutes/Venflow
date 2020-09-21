using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Dynamic;
using Venflow.Enums;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;

namespace Venflow.Commands
{
    internal class RelationPath
    {
        internal EntityRelation CurrentRelation { get; }
        internal List<RelationPath> TrailingPath { get; }

        internal RelationPath(EntityRelation currentRelation)
        {
            CurrentRelation = currentRelation;

            TrailingPath = new();
        }

        internal RelationPath AddToPath(EntityRelation relation, out bool isNew)
        {
            for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
            {
                var trailingPath = TrailingPath[pathIndex];

                if (trailingPath.CurrentRelation == relation)
                {
                    isNew = false;

                    return trailingPath;
                }
            }

            var path = new RelationPath(relation);

            TrailingPath.Add(path);

            isNew = true;

            return path;
        }

        internal RelationPath AddToPath<T>(EntityRelation relation, T value, out bool isNew)
        {
            for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
            {
                var trailingPath = TrailingPath[pathIndex];

                if (trailingPath.CurrentRelation == relation)
                {
                    isNew = false;

                    return trailingPath;
                }
            }

            var path = new RelationPath<T>(relation, value);

            TrailingPath.Add(path);

            isNew = true;

            return path;
        }
    }

    internal class RelationPath<T> : RelationPath
    {
        internal T Value { get; }

        internal RelationPath(EntityRelation currentRelation, T value) : base(currentRelation)
        {
            Value = value;
        }
    }

    internal class RelationBuilderValues
    {
        internal List<RelationPath> TrailingPath { get; }
        internal List<RelationPath> FlattenedPath { get; }

        private RelationPath _currentPath;

        internal RelationBuilderValues()
        {
            _currentPath = default!;
            TrailingPath = new();
            FlattenedPath = new();
        }

        internal EntityRelation[] GetFlattenedRelations()
        {
            var flattenedPathSpan = FlattenedPath.AsSpan();
            var entityRelations = new EntityRelation[flattenedPathSpan.Length];
            var entityRelationsSpan = entityRelations.AsSpan();

            for (int i = flattenedPathSpan.Length - 1; i >= 0; i--)
            {
                entityRelationsSpan[i] = flattenedPathSpan[i].CurrentRelation;
            }

            return entityRelations;
        }

        internal Entity BaseRelationWith<TRootEntity, TTarget>(Entity parent, Expression<Func<TRootEntity, TTarget>> propertySelector)
            where TRootEntity : class, new()
            where TTarget : class
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!parent.Relations!.TryGetValue(foreignProperty.Name, out var relation))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TRootEntity).Name}' isn't in any relation with the entity '{typeof(TRootEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            AddToPath(relation, true);

            return relation.RightEntity;
        }

        internal Entity BaseRelationWith<TRootEntity, TTarget, T>(Entity parent, Expression<Func<TRootEntity, TTarget>> propertySelector, T value)
            where TRootEntity : class, new()
            where TTarget : class
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!parent.Relations!.TryGetValue(foreignProperty.Name, out var relation))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TRootEntity).Name}' isn't in any relation with the entity '{typeof(TRootEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            AddToPath(relation, value, true);

            return relation.RightEntity;
        }

        internal Entity BaseAndWith<TRelationEntity, TTarget>(Entity parent, Expression<Func<TRelationEntity, TTarget>> propertySelector)
            where TRelationEntity : class, new()
            where TTarget : class
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!parent.Relations!.TryGetValue(foreignProperty.Name, out var relation))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TRelationEntity).Name}' isn't in any relation with the entity '{typeof(TRelationEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            AddToPath(relation, false);

            return relation.RightEntity;
        }

        internal Entity BaseAndWith<TRelationEntity, TTarget, T>(Entity parent, Expression<Func<TRelationEntity, TTarget>> propertySelector, T value)
            where TRelationEntity : class, new()
            where TTarget : class
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!parent.Relations!.TryGetValue(foreignProperty.Name, out var relation))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TRelationEntity).Name}' isn't in any relation with the entity '{typeof(TRelationEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            AddToPath(relation, value, false);

            return relation.RightEntity;
        }

        private void AddToPath<T>(EntityRelation relation, T value, bool newFullPath)
        {
            if (newFullPath)
            {
                for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
                {
                    var path = TrailingPath[pathIndex];

                    if (path.CurrentRelation == relation)
                    {
                        _currentPath = path;

                        return;
                    }
                }

                _currentPath = new RelationPath<T>(relation, value);

                TrailingPath.Add(_currentPath);

                FlattenedPath.Add(_currentPath);
            }
            else
            {
                _currentPath = _currentPath.AddToPath(relation, value, out var isNew);

                if (isNew)
                {
                    FlattenedPath.Add(_currentPath);
                }
            }
        }

        private void AddToPath(EntityRelation relation, bool newFullPath)
        {
            if (newFullPath)
            {
                for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
                {
                    var path = TrailingPath[pathIndex];

                    if (path.CurrentRelation == relation)
                    {
                        _currentPath = path;

                        return;
                    }
                }

                _currentPath = new RelationPath(relation);

                TrailingPath.Add(_currentPath);

                FlattenedPath.Add(_currentPath);
            }
            else
            {
                _currentPath = _currentPath.AddToPath(relation, out var isNew);

                if (isNew)
                {
                    FlattenedPath.Add(_currentPath);
                }
            }
        }
    }

    public interface IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IPreCommandBuilder<TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();
    }

    public interface IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();
    }

    public interface IBaseInsertRelationBuilder<TRelationEntity, TRootEntity> : IInsertCommandBuilder<TRootEntity>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
    {
        IBaseInsertRelationBuilder<TRootEntity, TRootEntity> InsertWithAll();

        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();
    }

    public interface IInsertRelationBuilder<TRelationEntity, TRootEntity> : IBaseInsertRelationBuilder<TRelationEntity, TRootEntity>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
    {
        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
    where TToEntity : class, new();

        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();
    }

    internal class InsertRelationBuilder<TRelationEntity, TRootEntity> : IInsertRelationBuilder<TRelationEntity, TRootEntity>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
    {
        private readonly Entity _parent;
        private readonly VenflowInsertCommandBuilder<TRootEntity> _commandBuilder;
        private readonly RelationBuilderValues _relationBuilder;

        public InsertRelationBuilder(Entity parent, VenflowInsertCommandBuilder<TRootEntity> commandBuilder, RelationBuilderValues relationBuilder)
        {
            _parent = parent;
            _commandBuilder = commandBuilder;
            _relationBuilder = relationBuilder;
        }

        public IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseRelationWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseRelationWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
           where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseRelationWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseAndWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseAndWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseAndWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        IInsertCommand<TRootEntity> ISpecficVenflowCommandBuilder<IInsertCommand<TRootEntity>>.Build()
            => _commandBuilder.Build();

        Task<int> IInsertCommandBuilder<TRootEntity>.InsertAsync(TRootEntity entity, CancellationToken cancellationToken)
             => _commandBuilder.InsertAsync(entity, cancellationToken);

        Task<int> IInsertCommandBuilder<TRootEntity>.InsertAsync(IList<TRootEntity> entities, CancellationToken cancellationToken)
             => _commandBuilder.InsertAsync(entities, cancellationToken);

        IBaseInsertRelationBuilder<TRootEntity, TRootEntity> IBaseInsertRelationBuilder<TRelationEntity, TRootEntity>.InsertWithAll()
             => _commandBuilder.InsertWithAll();
    }


    internal class QueryRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        private readonly Entity _parent;
        private readonly VenflowQueryCommandBuilder<TRootEntity, TReturn> _commandBuilder;
        private readonly RelationBuilderValues _relationBuilder;

        public QueryRelationBuilder(Entity parent, VenflowQueryCommandBuilder<TRootEntity, TReturn> commandBuilder, RelationBuilderValues relationBuilder)
        {
            _parent = parent;
            _commandBuilder = commandBuilder;
            _relationBuilder = relationBuilder;
        }

        public IQueryCommand<TRootEntity, TReturn> Build()
            => _commandBuilder.Build();

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_relationBuilder.BaseRelationWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_relationBuilder.BaseRelationWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_relationBuilder.BaseRelationWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_relationBuilder.BaseAndWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_relationBuilder.BaseAndWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
           => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_relationBuilder.BaseAndWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryCommandBuilder<TRootEntity, TReturn> TrackChanges(bool trackChanges = true)
             => _commandBuilder.TrackChanges(trackChanges);

#if !NET48
        [return: MaybeNull]
#endif
        public Task<TReturn> QueryAsync(CancellationToken cancellationToken = default)
            => _commandBuilder.QueryAsync(cancellationToken);

        public IBaseQueryRelationBuilder<TRootEntity, TRootEntity, TReturn> AddFormatter()
            => _commandBuilder.AddFormatter();
    }
}
