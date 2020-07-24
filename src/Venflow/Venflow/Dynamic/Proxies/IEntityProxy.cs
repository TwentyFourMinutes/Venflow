using System.Runtime.CompilerServices;

namespace Venflow.Dynamic.Proxies
{
    internal interface IEntityProxy<TEntity> where TEntity : class
    {
        ChangeTracker<TEntity> ChangeTracker { get; }
    }
}
