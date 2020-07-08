using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow
{
    internal static class DbConfigurationCache
    {
        internal static ConcurrentDictionary<Type, IReadOnlyDictionary<string, Entity>> EntitiesCache { get; } = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, Entity>>();

        internal static object BuildLocker { get; } = new object();
    }

    public abstract class DbConfiguration : IAsyncDisposable
    {
        internal IReadOnlyDictionary<string, Entity> Entities { get; private set; }
        internal bool IsBuild { get; private set; }

        internal string ConnectionString { get; }

        private NpgsqlConnection? _connection;

        protected DbConfiguration(string connectionString)
        {
            ConnectionString = connectionString;

            Build();
        }

        public void TrackChanges<TEntity>(ref TEntity entity) where TEntity : class
        {
            if (!Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.", nameof(TEntity));
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
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.", nameof(TEntity));
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
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.", nameof(TEntity));
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

        public void Build()
        {
            if (IsBuild)
                return;

            lock (DbConfigurationCache.BuildLocker)
            {
                if (IsBuild)
                    return;

                var type = this.GetType();

                if (DbConfigurationCache.EntitiesCache.TryGetValue(type, out var entities))
                {
                    Entities = entities;
                }
                else
                {
                    var dbConfigurator = new DbConfigurator();

                    Entities = dbConfigurator.BuildConfiguration(this, this.GetType());

                    DbConfigurationCache.EntitiesCache.TryAdd(type, Entities);
                }

                IsBuild = true;
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
