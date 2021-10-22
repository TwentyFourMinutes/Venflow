using System.Runtime.InteropServices;

namespace Reflow.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public readonly ref struct QueryBuilder<T> where T : class, new()
    {

    }

    public static class QueryBuilderExtensions
    {
        public static QueryBuilder<T> TrackChanges<T>(this QueryBuilder<T> builder) where T : class, new()
        {
            _ = builder;

            return default;
        }

        public static QueryBuilder<T> TrackChanges<T>(this QueryBuilder<T> builder, bool trackChanges) where T : class, new()
        {
            _ = builder;
            _ = trackChanges;

            return default;
        }

        public static Task<T?> SingleAsync<T>(this QueryBuilder<T> builder, CancellationToken cancellationToken = default) where T : class, new()
        {
            _ = builder;

            return Query.SingleAsync<T>(cancellationToken);
        }
    }
}
