using System.Text;

namespace Venflow.CodeFirst.Operations
{
    internal class DropTableMigration : IMigrationChange
    {
        public string Name { get; }

        public DropTableMigration(string name)
        {
            Name = name;
        }

        public void ApplyChanges(MigrationContext migrationContext, MigrationEntity? migrationEntity)
        {
            migrationContext.Entities.Remove(Name);
        }

        public void ApplyMigration(StringBuilder migration, MigrationEntity? migrationEntity)
        {
            throw new System.NotImplementedException();
        }
    }
}
