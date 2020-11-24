using System;
using System.Collections.Generic;
using System.Text;

namespace Venflow.Design
{
    public abstract class Migration
    {
        public abstract string Name { get; }
        public abstract string Checksum { get; }


        private readonly Dictionary<string, EntityMigration> _entityMigrations;

        protected Migration()
        {
            _entityMigrations = new();
        }

        public abstract void Changes();

        protected void Entity(string tableName, Action<EntityMigration> entityMigration)
            => entityMigration.Invoke(GetEntityMigration(tableName));

        private EntityMigration GetEntityMigration(string tableName)
        {
            if (!_entityMigrations.TryGetValue(tableName, out var entityMigration))
            {
                entityMigration = new EntityMigration(tableName);

                _entityMigrations.Add(tableName, entityMigration);
            }

            return entityMigration;
        }

        public void ApplyMigration(StringBuilder migrationSqlBuilder)
        {
            Changes();

            foreach (var entityMigration in _entityMigrations.Values)
            {
                foreach (var migrationChange in entityMigration.MigrationChanges)
                {
                    migrationChange.ApplyMigration(migrationSqlBuilder);
                }
            }
        }

        internal void ApplyChanges(MigrationContext migrationContext)
        {
            Changes();

            foreach (var entityMigration in _entityMigrations.Values)
            {
                foreach (var migrationChange in entityMigration.MigrationChanges)
                {
                    migrationChange.ApplyChanges(migrationContext);
                }
            }
        }
    }
}
