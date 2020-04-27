using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Venflow
{

    internal class ColumnDefinition
    {
        public string Name { get; set; }

        public ColumnDefinition(string name)
        {
            Name = name;
        }
    }
}
