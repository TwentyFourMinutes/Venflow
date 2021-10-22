using System.ComponentModel.DataAnnotations.Schema;

namespace Reflow.Modeling
{
    public interface IEntityBuilder<TEntity> : ILeftRelationBuilder<TEntity> where TEntity : class, new()
    {
        IEntityBuilder<TEntity> MapTable(string tableName);

        IPropertyBuilder MapId<TTarget>(Func<TEntity, TTarget> propertySelector, DatabaseGeneratedOption option = DatabaseGeneratedOption.Identity);
        IPropertyBuilder Column<TTarget>(Func<TEntity, TTarget> propertySelector);
        IPropertyBuilder Column<TTarget>(Func<TEntity, TTarget> propertySelector, string columnName);

        IEntityBuilder<TEntity> Ignore<TTarget>(Func<TEntity, TTarget> propertySelector);
    }
}
