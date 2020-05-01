namespace Venflow.Modeling
{
    internal class PrimaryColumnDefinition<TEntity> : ColumnDefinition<TEntity> where TEntity : class
    {
        public bool IsServerSideGenerated { get; set; }

        public PrimaryColumnDefinition(string name) : base(name)
        {
        }
    }
}
