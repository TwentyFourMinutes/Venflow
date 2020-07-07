using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Venflow.Dynamic")]

namespace Venflow.Modeling
{
    internal interface IEntityProxy<TEntity> where TEntity : class
    {
        ChangeTracker<TEntity> ChangeTracker { get; }
    }
}
