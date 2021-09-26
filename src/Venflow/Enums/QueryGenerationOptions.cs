namespace Venflow.Enums
{
    [Flags]
    internal enum QueryGenerationOptions : byte
    {
        None = 0,
        GenerateJoins = 1
    }
}
