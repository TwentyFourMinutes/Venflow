using System;
using Venflow.CodeFirst.Operations;

namespace Venflow.CodeFirst
{
    internal class MigrationColumn
    {
        internal string Name { get; }
        internal Type DataType { get; }
        internal ColumnDetails Details { get; }

        internal MigrationColumn(string name, Type dataType, ColumnDetails details)
        {
            Name = name;
            DataType = dataType;
            Details = details;
        }
    }
}
