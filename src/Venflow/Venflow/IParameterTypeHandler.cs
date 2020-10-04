using System;
using System.Runtime.CompilerServices;
using Npgsql;

namespace Venflow
{

    public interface IParameterTypeHandler
    {
        NpgsqlParameter Handle(string name, object val);
    }
}
