namespace Venflow.Modeling
{
    internal class ColumnInformation
    {
        internal uint? Precision { get; }
        internal uint? Scale { get; }
        internal string? Comment { get; }
        internal string? DefaultValue { get; }

        internal ColumnInformation(uint? precision, uint? scale, string? comment, string? defaultValue)
        {
            Precision = precision;
            Scale = scale;
            Comment = comment;
            DefaultValue = defaultValue;
        }
    }
}
