namespace Venflow.Modeling.Definitions
{
    internal class PrimaryColumnDefinition<TEntity> : ColumnDefinition<TEntity> where TEntity : class, new()
    {
        internal bool IsServerSideGenerated { get; set; }

        internal PrimaryColumnDefinition(string name) : base(name)
        {
        }
    }
}
