namespace Venflow.Dynamic.Proxies
{
    internal interface IEntityProxy<TEntity> where TEntity : class, new()
    {
        ChangeTracker<TEntity> ChangeTracker { get; }
    }
}
