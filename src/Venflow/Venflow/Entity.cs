using System.Text;

namespace Venflow
{
    internal class Entity<TEntity> : IEntity where TEntity : class
    {
        internal string TableName { get; }

        internal string ColumnsString { get; }
        internal string ColumnsStringWithPrimaryKey { get; }

        internal EntityColumn<TEntity>[] Columns { get; set; }

        internal PrimaryEntityColumn<TEntity> PrimaryColumn { get; set; }

        public Entity(string tableName, EntityColumn<TEntity>[] columns, PrimaryEntityColumn<TEntity> primaryColumn)
        {
            TableName = "\"" + tableName + "\"";
            Columns = columns;
            ColumnsStringWithPrimaryKey = BuildColumnsString(0, columns);
            ColumnsString = BuildColumnsString(1, columns);
            PrimaryColumn = primaryColumn;
        }

        private string BuildColumnsString(int offset, EntityColumn<TEntity>[] columns)
        {
            var sb = new StringBuilder();

            sb.Append("(");

            for (int i = offset; i < columns.Length; i++)
            {
                sb.Append('"');
                sb.Append(columns[i].ColumnName);
                sb.Append("\",");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");

            return sb.ToString();
        }
    }

    internal interface IEntity
    {
    }
}
