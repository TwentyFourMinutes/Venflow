using System.Text;

namespace Venflow.Design.Operations
{
    internal class CreateTableMigration : IMigrationChange
    {
        public MigrationEntity MigrationEntity { get; }

        internal string Name { get; }

        public CreateTableMigration(string name, MigrationEntity migrationEntity)
        {
            Name = name;
            MigrationEntity = migrationEntity;
        }

        public void ApplyChanges(MigrationContext migrationContext)
        {
            migrationContext.Entities.Add(Name, MigrationEntity);
        }

        public void ApplyMigration(StringBuilder migration)
        {
            migration.Append(@"CREATE TABLE """)
                     .Append(Name)
                     .AppendLine(@""" ();");
        }

        public void CreateMigration(StringBuilder migrationClass)
        {
            migrationClass.Append("migration.Create();");
        }
    }
}
