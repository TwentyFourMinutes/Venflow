using System.ComponentModel;

namespace Reflow
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Column
    {
        internal string Name { get; }
        internal ushort Index { get; }

        public Column(string name, ushort index)
        {
            Name = name;
            Index = index;
        }
    }
}
