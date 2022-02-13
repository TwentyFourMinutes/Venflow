namespace Reflow.Analyzer.Shared
{
    internal class AssemblyInfo
    {
        internal static readonly IReadOnlyList<byte> PublicKey = typeof(AssemblyInfo).Assembly
            .GetName()
            .GetPublicKey();
    }
}
