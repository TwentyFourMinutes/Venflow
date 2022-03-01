using System.Runtime.InteropServices;

namespace Reflow.Operations
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public readonly ref struct QueryBuilder<TEntity> where TEntity : class, new()
    {
        public QueryBuilder<TEntity> TrackChanges()
        {
            return default;
        }

        public QueryBuilder<TEntity> TrackChanges(bool trackChanges)
        {
            _ = trackChanges;

            return default;
        }

        public QueryBuilder<TEntity> Caching()
        {
            return default;
        }

        public QueryBuilder<TEntity> Caching(bool caching)
        {
            _ = caching;

            return default;
        }

        public QueryRelationBuilder<TEntity, TToEntity> Join<TToEntity>(
            Func<TEntity, TToEntity> with
        ) where TToEntity : class, new()
        {
            _ = with;

            return default;
        }

        public QueryRelationBuilder<TEntity, TToEntity> Join<TToEntity>(
            Func<TEntity, IList<TToEntity>> with
        ) where TToEntity : class, new()
        {
            _ = with;

            return default;
        }

        public ValueTask<TEntity?> SingleAsync(CancellationToken cancellationToken = default)
        {
            return Query.SingleAsync<TEntity>(false, cancellationToken);
        }

        public ValueTask<IList<TEntity>> ManyAsync(CancellationToken cancellationToken = default)
        {
            return Query.ManyAsync<TEntity>(cancellationToken);
        }
    }
}
