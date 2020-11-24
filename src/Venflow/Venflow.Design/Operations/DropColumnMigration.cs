using System.Text;

namespace Venflow.Design.Operations
{
    internal class DropColumnMigration : IMigrationChange
    {
        public MigrationEntity MigrationEntity { get; }

        internal string Name { get; }

        internal DropColumnMigration(string name, MigrationEntity migrationEntity)
        {
            Name = name;
            MigrationEntity = migrationEntity;
        }

        public void ApplyChanges(MigrationContext migrationContext)
        {
            MigrationEntity.Columns.Remove(Name);
        }

        public void ApplyMigration(StringBuilder migration)
        {
            migration.Append(@"ALTER TABLE """).Append(MigrationEntity.Name).Append(@""" DROP COLUMN """).Append(Name).AppendLine(@""";");
        }

        public void CreateMigration(StringBuilder migrationClass)
        {
            migrationClass.Append("migration.DropColumn(").Append(Name).Append(')');
        }
    }
}
