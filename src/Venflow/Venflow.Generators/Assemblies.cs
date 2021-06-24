using System.Reflection;

namespace Venflow.Generators
{
    internal static class Assemblies
    {
        private static readonly byte[] _venflowToken = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();

        internal static readonly AssemblyTokenInfo VenflowKeys = new AssemblyTokenInfo("Venflow.Keys", _venflowToken);
        internal static readonly AssemblyTokenInfo VenflowNewtonsoftJson = new AssemblyTokenInfo("Venflow.NewtonsoftJson", _venflowToken);
    }

    internal class AssemblyTokenInfo
    {
        internal string Name { get; }
        internal byte[] Token { get; }
        public AssemblyTokenInfo(string name, byte[] token)
        {
            Name = name;
            Token = token;
        }

    }
}
