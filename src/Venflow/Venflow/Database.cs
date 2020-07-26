using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
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

        public async Task<int> ExecuteAsync(string sql, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand(sql, GetConnection());

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<int> ExecuteAsync(string sql, IList<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand(sql, GetConnection());

            for (int i = 0; i < parameters.Count; i++)
            {
                command.Parameters.Add(parameters[i]);
            }

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<T> ExecuteAsync<T>(string sql, CancellationToken cancellationToken = default) where T : struct
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand(sql, GetConnection());

            return (T)await command.ExecuteScalarAsync(cancellationToken);
        }

        public async Task<T> ExecuteAsync<T>(string sql, IList<NpgsqlParameter> parameters, CancellationToken cancellationToken = default) where T : struct
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand(sql, GetConnection());

            for (int i = 0; i < parameters.Count; i++)
            {
                command.Parameters.Add(parameters[i]);
            }

            return (T)await command.ExecuteScalarAsync(cancellationToken);
        }

        public void TrackChanges<TEntity>(ref TEntity entity) where TEntity : class
        {
            if (!Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the Database.", typeof(TEntity).Name);
            }

            var entityConfiguration = (Entity<TEntity>)entityModel;

            entity = entityConfiguration.ApplyChangeTracking(entity);
        }

        public void TrackChanges<TEntity>(ref IList<TEntity> entities) where TEntity : class
        {
            if (!Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the Database.", typeof(TEntity).Name);
            }

            var entityConfiguration = (Entity<TEntity>)entityModel;

                for (int i = 0; i < entities.Count; i++)
                {
                    proxiedEntities.Add(entityConfiguration.ApplyChangeTracking(entities[i]));
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

        private ValueTask ValidateConnectionAsync()
        {
            var connection = GetConnection();

            if (connection.State == ConnectionState.Open)
                return default;

            if (connection.State == ConnectionState.Closed)
            {
                return new ValueTask(connection.OpenAsync());
            }
            else
            {
                throw new InvalidOperationException($"The current connection state is invalid. Expected: '{ConnectionState.Open}' or '{ConnectionState.Closed}'. Actual: '{connection.State}'.");
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
