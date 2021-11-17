using System.ComponentModel;
using System.Reflection;

namespace Reflow
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AssemblyRegister
    {
        public static Assembly? Assembly { get; set; } = null;
    }
}
