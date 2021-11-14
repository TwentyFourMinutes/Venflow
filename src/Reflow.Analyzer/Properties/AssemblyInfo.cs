namespace Reflow.Analyzer.Properties
{
    internal class AssemblyInfo
    {
        internal static readonly IReadOnlyList<byte> PublicKey = typeof(AssemblyInfo).Assembly
            .GetName()
            .GetPublicKey();
    }
}
