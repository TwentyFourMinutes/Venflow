using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Dynamic;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal readonly struct InsertCacheKey
    {
        internal IReadOnlyList<EntityRelation> Relations => _relations;

        private readonly IReadOnlyList<EntityRelation> _relations;

        internal InsertCacheKey(IReadOnlyList<EntityRelation> relations)
        {
            _relations = relations;
        }

        public bool Equals(
#if !NET48 
            [AllowNull] 
            #endif 
            InsertCacheKey y)
        {
            if (y._relations.Count != _relations.Count)
                return false;

            var relaionsSpan = ((List<EntityRelation>)_relations).AsSpan();
            var foreignRelaionsSpan = ((List<EntityRelation>)y._relations).AsSpan();

            for (int relationIndex = relaionsSpan.Length - 1; relationIndex >= 0; relationIndex--)
            {
                if (relaionsSpan[relationIndex].RelationId != foreignRelaionsSpan[relationIndex].RelationId)
                    return false;
            }

            return true;
        }

        public new int GetHashCode()
        {
            var hashCode = new HashCode();

            var relaionsSpan = ((List<EntityRelation>)_relations).AsSpan();

            for (int relationIndex = relaionsSpan.Length - 1; relationIndex >= 0; relationIndex--)
            {
                hashCode.Add(relaionsSpan[relationIndex].RelationId);
            }

            return hashCode.ToHashCode();
        }
    }

    internal class InsertCacheKeyComparer : IEqualityComparer<InsertCacheKey>
    {
        internal static InsertCacheKeyComparer Default { get; } = new InsertCacheKeyComparer();

        private InsertCacheKeyComparer()
        {

        }

        public bool Equals(
#if !NET48
            [AllowNull]
            #endif  
            InsertCacheKey x,
#if !NET48
            [AllowNull]
            #endif
            InsertCacheKey y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(
#if !NET48
            [DisallowNull]
            #endif  
            InsertCacheKey obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class VenflowInsertCommandBuilder<TEntity> : IBaseInsertRelationBuilder<TEntity, TEntity>
        where TEntity : class, new()
    {
        private RelationBuilderValues? _relationBuilderValues;
        private bool _disposeCommand;
        private bool _isFullInsert;

        private readonly NpgsqlCommand _command;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;

        internal VenflowInsertCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, bool disposeCommand)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _command = command;
            _disposeCommand = disposeCommand;
        }

        public IInsertCommand<TEntity> Build()
        {
            if (_relationBuilderValues is { })
            {
                var relations = new List<EntityRelation>();

                var relationBuilderValuesSpan = _relationBuilderValues.TrailingPath.AsSpan();

                for (int relationIndex = relationBuilderValuesSpan.Length - 1; relationIndex >= 0; relationIndex--)
                {
                    var relation = relationBuilderValuesSpan[relationIndex];

                    relations.Add(relation.CurrentRelation);

                    if (relation.TrailingPath.Count > 0)
                        FlattenRelations(relation, relations);
                }

                return new VenflowInsertCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand, new InsertCacheKey(relations), _isFullInsert);
            }

            return new VenflowInsertCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand, _isFullInsert);
        }

        private void FlattenRelations(RelationPath relationPath, List<EntityRelation> relations)
        {
            var relationPathSpan = relationPath.TrailingPath.AsSpan();

            for (int relationIndex = relationPathSpan.Length - 1; relationIndex >= 0; relationIndex--)
            {
                var relation = relationPathSpan[relationIndex];

                relations.Add(relation.CurrentRelation);

                if (relation.TrailingPath.Count > 0)
                    FlattenRelations(relation, relations);
            }
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _disposeCommand = true;

            return Build().InsertAsync(entity, cancellationToken);
        }

        public Task<int> InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _disposeCommand = true;

            return Build().InsertAsync(entities, cancellationToken);
        }

        public IBaseInsertRelationBuilder<TEntity, TEntity> InsertWithAll()
        {
            _isFullInsert = true;

            return this;
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.InsertWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues();

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, this, _relationBuilderValues).InsertWith(propertySelector);
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.InsertWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues();

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, this, _relationBuilderValues).InsertWith(propertySelector);
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.InsertWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues();

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, this, _relationBuilderValues).InsertWith(propertySelector);
        }
    }
}