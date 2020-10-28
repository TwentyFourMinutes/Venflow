using System;
using System.Text;

namespace Venflow.CodeFirst.Operations
{
    internal class CreateColumnMigration : IMigrationChange
    {
        internal string Name { get; }
        internal Type Type { get; }
        internal bool IsNullable { get; }

        internal CreateColumnMigration(string name, Type type, bool isNullable)
        {
            Name = name;
            Type = type;
            IsNullable = isNullable;
        }

        public void ApplyChanges(MigrationContext migrationContext, MigrationEntity migrationEntity)
        {
            migrationEntity.Columns.Add(Name, new MigrationColumn(Name, Type, IsNullable));
        }

        public void ApplyMigration(StringBuilder migration, MigrationEntity? migrationEntity)
        {
            migration.AppendLine($@"ALTER TABLE ""{migrationEntity.Name}"" ADD {Name} datatype; ")
        }
    }
}
