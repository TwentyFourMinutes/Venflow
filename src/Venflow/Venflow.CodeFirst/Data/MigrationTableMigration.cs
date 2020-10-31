using System;
using Venflow.CodeFirst.Operations;

namespace Venflow.CodeFirst.Data
{
    internal class MigrationTableMigration : Migration
    {
        public override void Changes()
        {
            Entity("Migrations", migration =>
            {
                migration.Create();

                migration.AddColumn(nameof(MigrationDatabaseEntity.Name), typeof(string), new ColumnDetails { IsPrimaryKey = true, IsNullable = false });
                migration.AddColumn(nameof(MigrationDatabaseEntity.Checksum), typeof(string), false);
                migration.AddColumn(nameof(MigrationDatabaseEntity.Timestamp), typeof(DateTime), false);
            });
        }
    }
}
