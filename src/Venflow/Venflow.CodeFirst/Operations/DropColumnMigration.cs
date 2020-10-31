using System.Text;

namespace Venflow.CodeFirst.Operations
{
    internal class DropColumnMigration : IMigrationChange
    {
        internal string Name { get; }

        internal DropColumnMigration(string name)
        {
            Name = name;
        }

        public void ApplyChanges(MigrationContext migrationContext, MigrationEntity migrationEntity)
        {
            migrationEntity.Columns.Remove(Name);
        }

        public void ApplyMigration(StringBuilder migration, MigrationEntity? migrationEntity)
        {
            throw new System.NotImplementedException();
        }
    }
}
