namespace Venflow.CodeFirst.Operations
{
    public class ColumnDetails
    {
        public uint? Precision { get; init; }
        public uint? Scale { get; init; }
        public bool IsPrimaryKey { get; init; }
        public bool IsNullable { get; init; }
    }
}
