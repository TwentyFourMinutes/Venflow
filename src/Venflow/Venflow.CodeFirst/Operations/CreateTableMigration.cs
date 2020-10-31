using System.Text;

namespace Venflow.CodeFirst.Operations
{
    internal class CreateTableMigration : IMigrationChange
    {
        public string Name { get; }

        public CreateTableMigration(string name)
        {
            Name = name;
        }

        public void ApplyChanges(MigrationContext migrationContext, MigrationEntity? migrationEntity)
        {
            migrationContext.Entities.Add(Name, new MigrationEntity(Name));
        }

        public void ApplyMigration(StringBuilder migration, MigrationEntity? migrationEntity)
        {
            migration.Append(@"CREATE TABLE """)
                     .Append(migrationEntity.Name)
                     .Append(@""";");
        }
    }
}
