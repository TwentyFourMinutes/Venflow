namespace Venflow.Design.Operations
{
    public class ColumnDetails
    {
        public uint? Precision { get; set; }
        public uint? Scale { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsNullable { get; set; }
    }
}
