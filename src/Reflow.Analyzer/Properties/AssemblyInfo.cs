namespace Reflow
{
    internal class AssemblyInfo
    {
        internal static readonly IReadOnlyList<byte> PublicKey = typeof(AssemblyInfo).Assembly
            .GetName()
            .GetPublicKey();
    }
}
