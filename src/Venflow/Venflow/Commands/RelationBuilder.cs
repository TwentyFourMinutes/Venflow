using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
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

        internal RelationPath AddToPath(EntityRelation relation)
        {
            for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
            {
                var trailingPath = TrailingPath[pathIndex];

                if (trailingPath.CurrentRelation == relation)
                {
                    return trailingPath;
                }
            }

            var path = new RelationPath(relation);

            TrailingPath.Add(path);

            return path;
        }
    }

    internal class RelationBuilderValues
    {
        internal List<RelationPath> TrailingPath { get; }

        private RelationPath _currentPath;

        internal RelationBuilderValues()
        {
            _currentPath = default!;
            TrailingPath = new();
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
            }
            else
            {
                _currentPath = _currentPath.AddToPath(relation);
            }
        }
    }

    public interface IBaseJoinRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IQueryCommandBuilder<TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        IJoinRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        IJoinRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        IJoinRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();
    }

    public interface IJoinRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IBaseJoinRelationBuilder<TRelationEntity, TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        IJoinRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        IJoinRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        IJoinRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
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
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseRelationWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseRelationWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_relationBuilder.BaseRelationWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        IInsertCommand<TRootEntity> ISpecficVenflowCommandBuilder<IInsertCommand<TRootEntity>>.Build()
            => _commandBuilder.Build();

        Task<int> IInsertCommandBuilder<TRootEntity>.InsertAsync(TRootEntity entity, CancellationToken cancellationToken)
             => _commandBuilder.InsertAsync(entity, cancellationToken);

        Task<int> IInsertCommandBuilder<TRootEntity>.InsertAsync(IList<TRootEntity> entities, CancellationToken cancellationToken)
             => _commandBuilder.InsertAsync(entities, cancellationToken);

        IBaseInsertRelationBuilder<TRootEntity, TRootEntity> IBaseInsertRelationBuilder<TRelationEntity, TRootEntity>.InsertWithAll()
             => _commandBuilder.InsertWithAll();
    }

    //internal abstract class RelationBuilder<TRelationEntity, TRootEntity>
    //    where TRelationEntity : class, new()
    //    where TRootEntity : class, new()
    //{
    //    internal RelationBuilderValues RelationBuilderValues { get; }

    //    private readonly Entity _parent;

    //    protected RelationBuilder(Entity parent)
    //    {
    //        _parent = parent;

    //        RelationBuilderValues = new();
    //    }

    //    protected RelationBuilder(Entity parent, RelationBuilderValues relationBuilderValues)
    //    {
    //        _parent = parent;
    //        RelationBuilderValues = relationBuilderValues;
    //    }

    //    public IJoinBuilder<TToEntity, TRootEntity> JoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
    //        where TToEntity : class, new()
    //        => BaseRelationWith<TToEntity, TToEntity>(_parent, propertySelector);

    //    public IJoinBuilder<TToEntity, TRootEntity> JoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
    //        where TToEntity : class, new()
    //        => BaseRelationWith<TToEntity, IList<TToEntity>>(_parent, propertySelector);

    //    public IJoinBuilder<TToEntity, TRootEntity> JoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
    //        where TToEntity : class, new()
    //        => BaseRelationWith<TToEntity, List<TToEntity>>(_parent, propertySelector);

    //    public IJoinBuilder<TToEntity, TRootEntity> ThenWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
    //        where TToEntity : class, new()
    //        => BaseAndWith<TToEntity, TToEntity>(_parent, propertySelector);

    //    public IJoinBuilder<TToEntity, TRootEntity> ThenWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
    //        where TToEntity : class, new()
    //        => BaseAndWith<TToEntity, IList<TToEntity>>(_parent, propertySelector);

    //    public IJoinBuilder<TToEntity, TRootEntity> ThenWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
    //        where TToEntity : class, new()
    //        => BaseAndWith<TToEntity, List<TToEntity>>(_parent, propertySelector);


    //}
}
