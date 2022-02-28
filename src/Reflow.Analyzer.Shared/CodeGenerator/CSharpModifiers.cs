namespace Reflow.Analyzer.CodeGenerator
{
    [Flags]
    public enum CSharpModifiers : ushort
    {
        None = 0,
        Private = 1 << 0,
        Protected = 1 << 1,
        Internal = 1 << 2,
        Public = 1 << 3,
        Partial = 1 << 4,
        Sealed = 1 << 5,
        Static = 1 << 6,
        Abstract = 1 << 7,
        Override = 1 << 8,
        Virtual = 1 << 9,
        Ref = 1 << 10,
        Async = 1 << 11,
        ReadOnly = 1 << 12,
    }
}
