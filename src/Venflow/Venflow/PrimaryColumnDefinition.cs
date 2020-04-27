using System;

namespace Venflow
{
    internal class PrimaryColumnDefinition<TEntity> : ColumnDefinition where TEntity : class
    {
        public Action<TEntity, object>? ValueWriter { get; set; }

        public bool IsServerSideGenerated { get; set; }

        public PrimaryColumnDefinition(string name) : base(name)
        {

        }
    }
}
