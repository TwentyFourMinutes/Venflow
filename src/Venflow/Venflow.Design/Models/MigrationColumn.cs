using System;
using Venflow.Design.Operations;

namespace Venflow.Design
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
