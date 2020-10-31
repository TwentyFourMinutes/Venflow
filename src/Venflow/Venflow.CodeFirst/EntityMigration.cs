using System;
using System.Collections.Generic;
using Venflow.CodeFirst.Operations;

namespace Venflow.CodeFirst
{
    public class EntityMigration
    {
        internal List<IMigrationChange> MigrationChanges { get; }
        internal string TableName { get; }
        public EntityMigration(string tableName)
        {
            TableName = tableName;
            MigrationChanges = MigrationChanges = new List<IMigrationChange>();
        }

        public void Create()
            => MigrationChanges.Add(new CreateTableMigration(TableName));

        public void Drop()
            => MigrationChanges.Add(new DropTableMigration(TableName));

        public void AddColumn(string name, Type type, bool isNullable)
            => MigrationChanges.Add(new CreateColumnMigration(name, type, new ColumnDetails { IsNullable = isNullable }));

        public void AddColumn(string name, Type type, ColumnDetails columnDetails)
            => MigrationChanges.Add(new CreateColumnMigration(name, type, columnDetails));

        public void DropColumn(string name)
            => MigrationChanges.Add(new DropColumnMigration(name));
    }
}
