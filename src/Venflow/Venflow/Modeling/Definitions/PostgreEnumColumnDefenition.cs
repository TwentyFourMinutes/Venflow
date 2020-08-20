namespace Venflow.Modeling.Definitions
{
    internal class PostgreEnumColumnDefenition<TEntity> : ColumnDefinition<TEntity> where TEntity : class, new()
    {
        internal PostgreEnumColumnDefenition(string name) : base(name)
        {
        }
    }
}
