using System.Text;

namespace Venflow.Design.Operations
{
    internal class DropTableMigration : IMigrationChange
    {
        public MigrationEntity MigrationEntity { get; }

        internal string Name { get; }

        internal DropTableMigration(string name, MigrationEntity migrationEntity)
        {
            Name = name;
            MigrationEntity = migrationEntity;
        }

        public void ApplyChanges(MigrationContext migrationContext)
        {
            migrationContext.Entities.Remove(Name);
        }

        public void ApplyMigration(StringBuilder migration)
        {
            migration.Append(@"DROP TABLE """).Append(Name).AppendLine(@""";");
        }

        public void CreateMigration(StringBuilder migrationClass)
        {
            migrationClass.Append("migration.Drop();");
        }
    }
}
