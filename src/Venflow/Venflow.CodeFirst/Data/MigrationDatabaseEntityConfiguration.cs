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

    public class MigrationDatabaseEntityConfiguration : EntityConfiguration<MigrationDatabaseEntity>
    {
        protected override void Configure(IEntityBuilder<MigrationDatabaseEntity> entityBuilder)
        {
            entityBuilder.MapToTable("_VenflowMigrations");
            entityBuilder.MapId(x => x.Name, DatabaseGeneratedOption.None);
        }
    }
}
