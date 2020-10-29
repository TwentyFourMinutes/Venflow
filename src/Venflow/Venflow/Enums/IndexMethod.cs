using System.ComponentModel.DataAnnotations.Schema;
using Npgsql;
using NpgsqlTypes;

namespace Venflow.Enums
{
    public enum IndexMethod
    {
        Btree,
        hash,
        Gist,
        Spgist,
        Gin,
        Brin
    }
}
