﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Venflow
{
    internal class Entity<TEntity> : IEntity where TEntity : class
    {
        internal string TableName { get; }

        internal string ColumnsString { get; }
        internal string ColumnsStringWithPrimaryKey { get; }

        internal DualKeyCollection<string, EntityColumn<TEntity>> Columns { get; set; }

        internal PrimaryEntityColumn<TEntity> PrimaryColumn { get; set; }

        public Entity(string tableName, DualKeyCollection<string, EntityColumn<TEntity>> columns, PrimaryEntityColumn<TEntity> primaryColumn)
        {
            TableName = "\"" + tableName + "\"";
            Columns = columns;
            ColumnsStringWithPrimaryKey = BuildColumnsString(0, columns.KeysTwo);
            ColumnsString = BuildColumnsString(1, columns.KeysTwo);
            PrimaryColumn = primaryColumn;
        }

        private string BuildColumnsString(int offset, ICollection<string> columnNames)
        {
            var sb = new StringBuilder();

            sb.Append("(");

            foreach (var columnName in columnNames.Skip(offset))
            {
                sb.Append('"');
                sb.Append(columnName);
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
