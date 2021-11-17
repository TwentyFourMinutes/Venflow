using System.ComponentModel;

namespace Reflow.Lambdas
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MethodLocation
    {
        internal Type Type { get; }
        internal string MethodName { get; }

        public MethodLocation(Type type, string methodName)
        {
            Type = type;
            MethodName = methodName;
        }
    }
}
