using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class QueryRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        private readonly Entity _root;
        private readonly Entity _parent;
        private readonly VenflowQueryCommandBuilder<TRootEntity, TReturn> _commandBuilder;
        private readonly RelationBuilderValues _relationBuilder;

        public QueryRelationBuilder(Entity parent, VenflowQueryCommandBuilder<TRootEntity, TReturn> commandBuilder, RelationBuilderValues relationBuilder)
        {
            _parent = _root = parent;
            _commandBuilder = commandBuilder;
            _relationBuilder = relationBuilder;
        }

        public QueryRelationBuilder(Entity root, Entity parent, VenflowQueryCommandBuilder<TRootEntity, TReturn> commandBuilder, RelationBuilderValues relationBuilder)
        {
            _root = root;
            _parent = parent;
            _commandBuilder = commandBuilder;
            _relationBuilder = relationBuilder;
        }

        public IQueryCommand<TRootEntity, TReturn> Build()
            => _commandBuilder.Build();

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_root, _relationBuilder.BaseRelationWith(_root, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_root, _relationBuilder.BaseRelationWith(_root, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_root, _relationBuilder.BaseRelationWith(_root, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_root, _relationBuilder.BaseAndWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
            => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_root, _relationBuilder.BaseAndWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

        public IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
           => new QueryRelationBuilder<TToEntity, TRootEntity, TReturn>(_root, _relationBuilder.BaseAndWith(_parent, propertySelector, joinBehaviour), _commandBuilder, _relationBuilder);

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
