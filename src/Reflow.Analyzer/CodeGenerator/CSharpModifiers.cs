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
        Sealed = 1 << 4,
        Static = 1 << 5,
        Abstract = 1 << 6,
        Override = 1 << 7,
        Virtual = 1 << 8,
        Ref = 1 << 9
    }
}
