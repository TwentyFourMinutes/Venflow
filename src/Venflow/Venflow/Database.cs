using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow
{
    internal static class DatabaseConfigurationCache
    {
        internal static ConcurrentDictionary<Type, DatabaseConfiguration> DatabaseConfigurations { get; } = new ConcurrentDictionary<Type, DatabaseConfiguration>();

        internal static object BuildLocker { get; } = new object();
    }

    public abstract class Database : IAsyncDisposable
    {
        internal IReadOnlyDictionary<string, Entity> Entities { get; private set; }

        internal string ConnectionString { get; }

        private NpgsqlConnection? _connection;

        protected Database(string connectionString)
        {
            ConnectionString = connectionString;

            Build();
        }

        public void TrackChanges<TEntity>(ref TEntity entity) where TEntity : class
        {
            if (!Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the Database.", nameof(TEntity));
            }

            var entityConfiguration = (Entity<TEntity>)entityModel;

            if (entityConfiguration.ChangeTrackerApplier is { })
            {
                entity = entityConfiguration.ApplyChangeTracking(entity);
            }
        }

        public void TrackChanges<TEntity>(ref IList<TEntity> entities) where TEntity : class
        {
            if (!Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the Database.", nameof(TEntity));
            }

            var entityConfiguration = (Entity<TEntity>)entityModel;

            if (entityConfiguration.ChangeTrackerApplier is { })
            {
                var proxiedEntities = new List<TEntity>();

                for (int i = 0; i < entities.Count; i++)
                {
                    proxiedEntities.Add(entityConfiguration.ApplyChangeTracking(entities[i]));
                }

                entities = proxiedEntities;
            }
        }

        public void TrackChanges<TEntity>(ref IEnumerable<TEntity> entities) where TEntity : class
        {
            if (!Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the Database.", nameof(TEntity));
            }

            var entityConfiguration = (Entity<TEntity>)entityModel;

            if (entityConfiguration.ChangeTrackerApplier is { })
            {
                var proxiedEntities = new List<TEntity>();

                foreach (var entity in entities)
                {
                    proxiedEntities.Add(entityConfiguration.ApplyChangeTracking(entity));
                }

                entities = proxiedEntities;
            }
        }

        public NpgsqlConnection GetConnection()
        {
            if (_connection is { })
                return _connection;

            return _connection = new NpgsqlConnection(ConnectionString);
        }

        private void Build()
        {
            var type = this.GetType();

            if (DatabaseConfigurationCache.DatabaseConfigurations.TryGetValue(type, out var configuration))
            {
                Entities = configuration.Entities;

                configuration.InstantiateDatabase(this);

                return;
            }

            lock (DatabaseConfigurationCache.BuildLocker)
            {
                if (!DatabaseConfigurationCache.DatabaseConfigurations.TryGetValue(type, out configuration))
                {
                    var dbConfigurator = new DatabaseConfigurationFactory();

                    configuration = dbConfigurator.BuildConfiguration(this.GetType());

                    DatabaseConfigurationCache.DatabaseConfigurations.TryAdd(type, configuration);
                }

                Entities = configuration.Entities;

                configuration.InstantiateDatabase(this);
            }
        }

        public ValueTask DisposeAsync()
        {
            if (_connection is { })
            {
                return _connection.DisposeAsync();
            }

            return new ValueTask();
        }
    }
}
