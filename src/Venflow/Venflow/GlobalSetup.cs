using Venflow.Mappers;

namespace Venflow
{
    internal static class GlobalSetup
    {
        internal static void Apply()
        {
            UInt64HandlerFactory.ApplyMapping();
        }
    }
}
