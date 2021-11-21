using System.Runtime.InteropServices;

namespace Reflow.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public readonly ref struct QueryBuilder<TEntity> where TEntity : class, new()
    {
    }

    public static class QueryBuilderExtensions
    {
        public static QueryBuilder<TEntity> TrackChanges<TEntity>(
            this QueryBuilder<TEntity> builder
        ) where TEntity : class, new()
        {
            _ = builder;

            return default;
        }

        public static QueryBuilder<TEntity> TrackChanges<TEntity>(
            this QueryBuilder<TEntity> builder,
            bool trackChanges
        ) where TEntity : class, new()
        {
            _ = builder;
            _ = trackChanges;

            return default;
        }

        public static Task<TEntity?> SingleAsync<TEntity>(
            this QueryBuilder<TEntity> builder,
            CancellationToken cancellationToken = default
        ) where TEntity : class, new()
        {
            _ = builder;

            return Query.SingleAsync<TEntity>(cancellationToken);
        }

        public static Task<IList<TEntity>> ManyAsync<TEntity>(
            this QueryBuilder<TEntity> builder,
            CancellationToken cancellationToken = default
        ) where TEntity : class, new()
        {
            _ = builder;

            return Query.ManyAsync<TEntity>(cancellationToken);
        }
    }
}
