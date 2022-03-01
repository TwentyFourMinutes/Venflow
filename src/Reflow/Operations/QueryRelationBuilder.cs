using System.Runtime.InteropServices;

namespace Reflow.Operations
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public readonly ref struct QueryRelationBuilder<TRootEntity, TCurrentEntity>
        where TRootEntity : class, new()
        where TCurrentEntity : class, new()
    {
        public QueryRelationBuilder<TRootEntity, TToEntity> Join<TToEntity>(
            Func<TCurrentEntity, TToEntity> with
        ) where TToEntity : class, new()
        {
            _ = with;

            return default;
        }

        public QueryRelationBuilder<TRootEntity, TToEntity> Join<TToEntity>(
            Func<TCurrentEntity, IList<TToEntity>> with
        ) where TToEntity : class, new()
        {
            _ = with;

            return default;
        }

        public QueryRelationBuilder<TRootEntity, TToEntity> ThenJoin<TToEntity>(
            Func<TCurrentEntity, TToEntity> with
        ) where TToEntity : class, new()
        {
            _ = with;

            return default;
        }

        public QueryRelationBuilder<TRootEntity, TToEntity> ThenJoin<TToEntity>(
            Func<TCurrentEntity, IList<TToEntity>> with
        ) where TToEntity : class, new()
        {
            _ = with;

            return default;
        }

        public ValueTask<TRootEntity?> SingleAsync(CancellationToken cancellationToken = default)
        {
            return Query.SingleAsync<TRootEntity>(true, cancellationToken);
        }

        public ValueTask<IList<TRootEntity>> ManyAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Query.ManyAsync<TRootEntity>(cancellationToken);
        }
    }
}
