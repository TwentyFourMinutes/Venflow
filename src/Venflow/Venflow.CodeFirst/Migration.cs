using System;
using System.Collections.Generic;

namespace Venflow.CodeFirst
{
    public abstract class Migration
    {
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

        public void ApplyMigration()
        {
            var yeet = new MigrationContext();

            ApplyMigration(yeet);
        }

        internal void ApplyMigration(MigrationContext migrationContext)
        {
            Changes();

            foreach (var entityMigration in _entityMigrations)
            {
                foreach (var migrationChange in entityMigration.MigrationChanges)
                {
                    migrationChange.ApplyChanges(migrationContext, migrationContext.Entities.GetValueOrDefault(entityMigration.TableName));
                }
            }
        }
    }
}
