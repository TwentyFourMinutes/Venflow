using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.CodeFirst
{

    internal class MigrationDatabase : Database
    {
        public Table<MigrationDatabaseEntity> Migrations { get; set; }

        public MigrationDatabase(string connectionString) : base(connectionString)
        {

        }
    }
}
