using System.Reflection;

namespace Reflow
{
    internal static class Entities
    {
        internal static Dictionary<Type, Entity> Data { get; }

        static Entities()
        {
            Data =
                (Dictionary<Type, Entity>)AssemblyRegister.Assembly!.GetType(
                    "Reflow.EntityData"
                )!.GetField("Data", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        }
    }
}
