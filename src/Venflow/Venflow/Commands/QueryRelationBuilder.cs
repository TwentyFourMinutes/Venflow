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

        internal QueryRelationBuilder(Entity parent, VenflowQueryCommandBuilder<TRootEntity, TReturn> commandBuilder, RelationBuilderValues relationBuilder)
        {
            _parent = _root = parent;
            _commandBuilder = commandBuilder;
            _relationBuilder = relationBuilder;
        }

        internal QueryRelationBuilder(Entity root, Entity parent, VenflowQueryCommandBuilder<TRootEntity, TReturn> commandBuilder, RelationBuilderValues relationBuilder)
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

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenLeftWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenLeftWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenLeftWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenRightWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenRightWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenRightWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenFullWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.FullJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenFullWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.FullJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.ThenFullWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            => ThenWith(propertySelector, JoinBehaviour.FullJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.LeftJoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.LeftJoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.LeftJoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.RightJoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.RightJoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.RightJoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.FullJoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.FullJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.FullJoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.FullJoin);

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>.FullJoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
            => JoinWith(propertySelector, JoinBehaviour.FullJoin);

        public IQueryCommandBuilder<TRootEntity, TReturn> TrackChanges(bool trackChanges = true)
            => _commandBuilder.TrackChanges(trackChanges);

        public IQueryCommandBuilder<TRootEntity, TReturn> LogTo(bool shouldLog = true)
            => _commandBuilder.LogTo(shouldLog);

        public IQueryCommandBuilder<TRootEntity, TReturn> LogTo(Action<string> logger, bool includeSensitiveData)
            => _commandBuilder.LogTo(logger, includeSensitiveData);

        public IQueryCommandBuilder<TRootEntity, TReturn> LogTo(params (Action<string> logger, bool includeSensitiveData)[] loggers)
            => _commandBuilder.LogTo(loggers);

#if !NET48
        [return: MaybeNull]
#endif
        public Task<TReturn> QueryAsync(CancellationToken cancellationToken = default)
            => _commandBuilder.QueryAsync(cancellationToken);

        public IBaseQueryRelationBuilder<TRootEntity, TRootEntity, TReturn> AddFormatter()
            => _commandBuilder.AddFormatter();
    }
}
