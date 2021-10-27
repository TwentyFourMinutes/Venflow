namespace Reflow.Modeling
{
    public interface IEntityConfiguration<TEntity> where TEntity : class, new()
    {
        void Configure(IEntityBuilder<TEntity> entityBuilder);
    }
}
