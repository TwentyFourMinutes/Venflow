﻿using System.Linq.Expressions;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class InsertRelationBuilder<TRelationEntity, TRootEntity> : IInsertRelationBuilder<TRelationEntity, TRootEntity>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
    {
        private readonly Entity _root;
        private readonly Entity _parent;
        private readonly VenflowInsertCommandBuilder<TRootEntity> _commandBuilder;
        private readonly RelationBuilderValues _relationBuilder;

        internal InsertRelationBuilder(Entity root, Entity parent, VenflowInsertCommandBuilder<TRootEntity> commandBuilder, RelationBuilderValues relationBuilder)
        {
            _root = root;
            _parent = parent;
            _commandBuilder = commandBuilder;
            _relationBuilder = relationBuilder;
        }

        public IInsertRelationBuilder<TToEntity, TRootEntity> With<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_root, _relationBuilder.BaseRelationWith(_root, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> With<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_root, _relationBuilder.BaseRelationWith(_root, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> With<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
           where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_root, _relationBuilder.BaseRelationWith(_root, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_root, _relationBuilder.BaseAndWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_root, _relationBuilder.BaseAndWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        public IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new()
            => new InsertRelationBuilder<TToEntity, TRootEntity>(_root, _relationBuilder.BaseAndWith(_parent, propertySelector), _commandBuilder, _relationBuilder);

        IInsertCommand<TRootEntity> ISpecficVenflowCommandBuilder<IInsertCommand<TRootEntity>, IBaseInsertRelationBuilder<TRootEntity, TRootEntity>>.Build()
            => _commandBuilder.Build();

        public IBaseInsertRelationBuilder<TRootEntity, TRootEntity> Log(bool shouldLog = true)
            => _commandBuilder.Log(shouldLog);

        public IBaseInsertRelationBuilder<TRootEntity, TRootEntity> LogTo(LoggerCallback logger)
            => _commandBuilder.LogTo(logger);

        public IBaseInsertRelationBuilder<TRootEntity, TRootEntity> LogTo(params LoggerCallback[] loggers)
            => _commandBuilder.LogTo(loggers);

        Task<int> IInsertCommandBuilder<TRootEntity>.InsertAsync(TRootEntity entity, CancellationToken cancellationToken)
             => _commandBuilder.InsertAsync(entity, cancellationToken);

        Task<int> IInsertCommandBuilder<TRootEntity>.InsertAsync(IList<TRootEntity> entities, CancellationToken cancellationToken)
             => _commandBuilder.InsertAsync(entities, cancellationToken);

        IBaseInsertRelationBuilder<TRootEntity, TRootEntity> IBaseInsertRelationBuilder<TRelationEntity, TRootEntity>.WithAll()
             => _commandBuilder.WithAll();
    }
}
