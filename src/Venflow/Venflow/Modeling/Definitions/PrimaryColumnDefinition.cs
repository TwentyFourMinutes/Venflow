namespace Venflow.Modeling.Definitions
{
    internal class PrimaryColumnDefinition<TEntity> : ColumnDefinition<TEntity> where TEntity : class
    {
        internal bool IsServerSideGenerated { get; set; }

        internal PrimaryColumnDefinition(string name) : base(name)
        {
        }
    }
}
