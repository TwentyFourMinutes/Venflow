using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.Commands
{

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
}
