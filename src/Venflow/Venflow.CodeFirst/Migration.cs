using System;
using System.Collections.Generic;
using System.Text;

namespace Venflow.CodeFirst
{
    public abstract class Migration
    {
        public abstract string Name { get; }

        private readonly List<EntityMigration> _entityMigrations;

        protected Migration()
        {
            _entityMigrations = new();
        }

        public abstract void Changes();

        protected void Entity(string tableName, Action<EntityMigration> entityMigration)
            => entityMigration.Invoke(GetEntityMigration(tableName));

        private EntityMigration GetEntityMigration(string tableName)
        {
            var entityMigration = new EntityMigration(tableName);

            _entityMigrations.Add(entityMigration);

            return entityMigration;
        }

        public StringBuilder ApplyMigration()
        {
            var migrationSqlBuilder = new StringBuilder();

            ApplyMigration(migrationSqlBuilder);

            return migrationSqlBuilder;
        }

        internal void ApplyMigration(StringBuilder migrationSqlBuilder)
        {
            Changes();

            var migrationContext = new MigrationContext();

            foreach (var entityMigration in _entityMigrations)
            {
                foreach (var migrationChange in entityMigration.MigrationChanges)
                {
                    migrationChange.ApplyChanges(migrationContext);
                    migrationChange.ApplyMigration(migrationSqlBuilder);
                }
            }
        }

        internal void ApplyChanges(MigrationContext migrationContext)
        {
            Changes();

            foreach (var entityMigration in _entityMigrations)
            {
                foreach (var migrationChange in entityMigration.MigrationChanges)
                {
                    migrationChange.ApplyChanges(migrationContext);
                }
            }
        }
    }
}
