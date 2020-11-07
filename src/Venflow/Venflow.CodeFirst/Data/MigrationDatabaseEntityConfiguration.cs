using System.ComponentModel.DataAnnotations.Schema;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.CodeFirst.Data
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
