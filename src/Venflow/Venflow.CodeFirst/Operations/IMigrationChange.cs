using System.Text;

namespace Venflow.CodeFirst.Operations
{
    internal interface IMigrationChange
    {
        MigrationEntity MigrationEntity { get; }

        void ApplyChanges(MigrationContext migrationContext);

        void ApplyMigration(StringBuilder migration);

        void CreateMigration(StringBuilder migrationClass);
    }
}
