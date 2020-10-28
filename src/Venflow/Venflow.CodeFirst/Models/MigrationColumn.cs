using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.CodeFirst
{
    internal class MigrationColumn
    {
        internal string Name { get; }
        internal Type DataType { get; }
        internal bool IsNullable { get; }

        internal MigrationColumn(string name, Type dataType, bool isNullable)
        {
            Name = name;
            DataType = dataType;
            IsNullable = isNullable;
        }
    }
}
