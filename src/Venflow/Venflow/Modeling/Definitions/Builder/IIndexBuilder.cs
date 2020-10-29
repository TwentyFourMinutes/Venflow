using Venflow.Enums;

namespace Venflow.Modeling.Definitions.Builder
{
    public interface IIndexBuilder<TEntity>
        where TEntity : class, new()
    {
        IIndexBuilder<TEntity> HasName(string name);
        IIndexBuilder<TEntity> IsUnique();
        IIndexBuilder<TEntity> IsConcurrent();
        IIndexBuilder<TEntity> WithMethod(IndexMethod indexMethod);
        IIndexBuilder<TEntity> WithSortOder(IndexSortOrder order);
        IIndexBuilder<TEntity> WithNullSortOrder(IndexNullSortOrder order);
    }
}
