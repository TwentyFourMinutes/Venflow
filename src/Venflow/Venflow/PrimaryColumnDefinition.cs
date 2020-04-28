using System;

namespace Venflow
{
    internal class PrimaryColumnDefinition<TEntity> : ColumnDefinition<TEntity> where TEntity : class
    {
        public Action<TEntity, object> PrimaryKeyWriter { get; set; }

        public bool IsServerSideGenerated { get; set; }

        public PrimaryColumnDefinition(string name, Action<TEntity, object> primaryKeyWriter) : base(name)
        {
            PrimaryKeyWriter = primaryKeyWriter;
        }
    }
}
