using System;
using System.Collections.Generic;
using Venflow.CodeFirst.Operations;

namespace Venflow.CodeFirst
{
    public class EntityMigration
    {
        internal List<IMigrationChange> MigrationChanges { get; }
        internal string TableName { get; }

        internal MigrationEntity MigrationEntity { get; }

        public EntityMigration(string tableName)
        {
            TableName = tableName;
            MigrationChanges = MigrationChanges = new List<IMigrationChange>();

            MigrationEntity = new(tableName);
        }

        public void Create()
            => MigrationChanges.Add(new CreateTableMigration(TableName, MigrationEntity));

        public void Drop()
            => MigrationChanges.Add(new DropTableMigration(TableName, MigrationEntity));

        public void AddColumn(string name, Type type, bool isNullable)
            => MigrationChanges.Add(new CreateColumnMigration(name, type, new ColumnDetails { IsNullable = isNullable }, MigrationEntity));

        public void AddColumn(string name, Type type, ColumnDetails columnDetails)
            => MigrationChanges.Add(new CreateColumnMigration(name, type, columnDetails, MigrationEntity));

        public void DropColumn(string name)
            => MigrationChanges.Add(new DropColumnMigration(name, MigrationEntity));
    }
}
