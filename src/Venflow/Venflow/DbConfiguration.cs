using Npgsql;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Modeling
{
    public abstract class DbConfiguration
    {
        internal string ConnectionString { get; }
        internal bool UseLazyEntityEvaluation { get; }

        internal IReadOnlyDictionary<string, Entity> Entities { get; private set; }
        internal IReadOnlyDictionary<string, Entity> TableEntities { get; private set; }

        internal bool IsBuild { get; private set; }

        protected DbConfiguration(string connectionString, bool useLazyEntityEvaluation = false)
        {
            ConnectionString = connectionString;
            UseLazyEntityEvaluation = useLazyEntityEvaluation;
            Entities = null!;
            TableEntities = null!;
        }

        public async ValueTask<VenflowDbConnection> NewConnectionScopeAsync(bool openConnection = true, CancellationToken cancellationToken = default)
        {
            if (!this.IsBuild)
                Build();

            var connection = new NpgsqlConnection(ConnectionString);

            if (openConnection)
            {
                await connection.OpenAsync(cancellationToken);
            }

            return new VenflowDbConnection(this, connection);
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

        protected abstract void Configure(DbConfigurator dbConfigurator);

        public void Build()
        {
            if (IsBuild)
                return;

            var dbConfigurator = new DbConfigurator();

            Configure(dbConfigurator);

            Entities = dbConfigurator.BuildConfiguration();

            var tableEntities = new Dictionary<string, Entity>();

            foreach (var entity in Entities.Values)
            {
                tableEntities.Add(entity.TableName, entity);
            }

            TableEntities = new ReadOnlyDictionary<string, Entity>(tableEntities);

            IsBuild = true;
        }
    }
}
