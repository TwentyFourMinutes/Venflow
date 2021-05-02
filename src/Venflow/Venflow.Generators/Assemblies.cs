using System.Reflection;

namespace Venflow.Generators
{
    internal static class Assemblies
    {
        private static readonly byte[] _venflowToken = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();

        internal static readonly AssemblyTokenInfo Venflow = new AssemblyTokenInfo("Venflow", _venflowToken);
        internal static readonly AssemblyTokenInfo VenflowNewtonsoftJson = new AssemblyTokenInfo("Venflow.NewtonsoftJson", _venflowToken);
        internal static readonly AssemblyTokenInfo NewtonsoftJson = new AssemblyTokenInfo("Newtonsoft.Json", new byte[] { 48, 173, 79, 230, 178, 166, 174, 237 });
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
