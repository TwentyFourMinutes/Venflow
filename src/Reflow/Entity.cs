using System.ComponentModel;

namespace Reflow
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Entity
    {
        internal Dictionary<string, Column> Columns { get; }

        public Entity(Dictionary<string, Column> columns)
        {
            Columns = columns;
        }
    }
}
