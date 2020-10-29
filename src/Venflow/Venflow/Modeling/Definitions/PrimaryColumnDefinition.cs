namespace Venflow.Modeling.Definitions
{
    internal class PrimaryColumnDefinition : ColumnDefinition
    {
        internal bool IsServerSideGenerated { get; set; }

        internal PrimaryColumnDefinition(string name) : base(name)
        {
        }
    }
}
