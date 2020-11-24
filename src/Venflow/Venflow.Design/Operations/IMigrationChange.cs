using System.Text;

namespace Venflow.Design.Operations
{
    internal interface IMigrationChange
    {
        MigrationEntity MigrationEntity { get; }

        void ApplyChanges(MigrationContext migrationContext);

        void ApplyMigration(StringBuilder migration);

        void CreateMigration(StringBuilder migrationClass);
    }
}
