using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Venflow.Modeling.ProxyTypes")]

namespace Venflow.Modeling
{
    internal interface IEntityProxy<TEntity> where TEntity : class
    {
        ChangeTracker<TEntity> ChangeTracker { get; }
    }
}
