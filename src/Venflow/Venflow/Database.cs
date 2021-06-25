using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Venflow.Enums;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow
{
    internal static class DatabaseConfigurationCache
    {
        internal static ConcurrentDictionary<Type, DatabaseConfiguration> DatabaseConfigurations { get; } = new ConcurrentDictionary<Type, DatabaseConfiguration>();

        internal static Dictionary<Type, Entity> CustomEntities { get; } = new Dictionary<Type, Entity>(0);

        internal static object BuildLocker { get; } = new object();
    }

    /// <summary>
    /// A <see cref="Database"/> instance represents a session with the database and can be used to perform CRUD operations with your tables and entities.
    /// </summary>
    /// <remarks>
    /// Typically you create a class that derives from <see cref="Database"/> and contains <see cref="Table{TEntity}"/> properties for each entity in the Database. All the <see cref="Table{TEntity}"/> properties must have a public setter, they are automatically initialized when the instance of the derived type is created.
    /// </remarks>
    public abstract class Database : IAsyncDisposable, IDisposable
    {
        internal IReadOnlyDictionary<string, Entity> Entities { get; private set; }
        internal LoggingBehavior DefaultLoggingBehavior { get; }
        internal bool HasActiveTransaction => _activeTransaction is not null && !_activeTransaction.IsDisposed;

        internal string ConnectionString { get; }

        private NpgsqlConnection? _connection;
        private DatabaseTransaction? _activeTransaction;

        private readonly IReadOnlyList<LoggerCallback> _loggers;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class using the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to your PostgreSQL Database.</param>
        protected Database(string connectionString)
        {
            ConnectionString = connectionString;
            _loggers = Array.Empty<LoggerCallback>();

            Build();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class using the specified <paramref name="optionsBuilder"/>.
        /// </summary>
        /// <param name="optionsBuilder">The options builder containing all the necessary information for the <see cref="Database"/> instance.</param>
        protected Database(DatabaseOptionsBuilder optionsBuilder)
        {
            ConnectionString = optionsBuilder.ConnectionString;
            DefaultLoggingBehavior = optionsBuilder.DefaultLoggingBehavior;
            _loggers = optionsBuilder.Loggers;

            Build();
        }

        /// <summary>
        /// Asynchronously begins a new transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created transaction.</returns>
        /// <remarks>Be aware, that this method will not create a new transaction on every call. It will only create a new one, if the old one is disposed or not available.</remarks>
        public async ValueTask<IDatabaseTransaction> BeginTransactionAsync(
#if !NET48
            CancellationToken cancellationToken = default
#endif
            )
        {
            await ValidateConnectionAsync();

#if NET48
            return !HasActiveTransaction ? _activeTransaction : _activeTransaction = new DatabaseTransaction(GetConnection().BeginTransaction());
#else
            return !HasActiveTransaction ? _activeTransaction : _activeTransaction = new DatabaseTransaction(await GetConnection().BeginTransactionAsync(cancellationToken));
#endif
        }

        /// <summary>
        /// Asynchronously begins a new transaction.
        /// </summary>
        /// <param name="isolationLevel">The isolation level under which the transaction should run.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created transaction.</returns>
        /// <remarks>Be aware, that this method will not create a new transaction on every call. It will only create a new one, if the old one is disposed or not available.</remarks>
        public async ValueTask<IDatabaseTransaction> BeginTransactionAsync(IsolationLevel isolationLevel
#if !NET48
            , CancellationToken cancellationToken = default
#endif
            )
        {
            await ValidateConnectionAsync();

#if NET48
            return !HasActiveTransaction ? _activeTransaction : _activeTransaction = new DatabaseTransaction(GetConnection().BeginTransaction());
#else
            return !HasActiveTransaction ? _activeTransaction : _activeTransaction = new DatabaseTransaction(await GetConnection().BeginTransactionAsync(isolationLevel, cancellationToken));
#endif
        }

        internal async ValueTask<IDatabaseTransaction> GetOrCreateTransactionAsync(
#if !NET48
            CancellationToken cancellationToken = default
#endif
            )
        {
            if (HasActiveTransaction)
            {
                return _activeTransaction;
            }

#if NET48
            return _activeTransaction = new DatabaseTransaction(GetConnection().BeginTransaction());
#else
            return _activeTransaction = new DatabaseTransaction(await GetConnection().BeginTransactionAsync(cancellationToken));
#endif
        }

        /// <summary>
        /// Asynchronously executes a command against the current Database. As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments.
        /// </summary>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        /// <remarks>This method represents a <see cref="NpgsqlCommand.ExecuteNonQueryAsync(CancellationToken)"/> call.</remarks>
        public async Task<int> ExecuteAsync(string sql, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand(sql, GetConnection());

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a command against the current Database. As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments.
        /// </summary>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="parameters">The SQL Parameters which are being used for the current command.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        /// <remarks>This method represents a <see cref="NpgsqlCommand.ExecuteNonQueryAsync(CancellationToken)"/> call.</remarks>
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

        /// <summary>
        /// Asynchronously executes a command against the current Database. As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments.
        /// </summary>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="parameters">The SQL Parameters which are being used for the current command.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        /// <remarks>This method represents a <see cref="System.Data.Common.DbCommand.ExecuteNonQueryAsync()"/> call.</remarks>
        public async Task<int> ExecuteAsync(string sql, params NpgsqlParameter[] parameters)
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand(sql, GetConnection());

            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.Add(parameters[i]);
            }

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Asynchronously executes a command against the current Database. This method does automatically parameterize queries from an interpolated string.
        /// </summary>
        /// <param name="sql">The interpolated SQL to execute.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        /// <remarks>This method represents a <see cref="System.Data.Common.DbCommand.ExecuteNonQueryAsync()"/> call.</remarks>
        public async Task<int> ExecuteInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand();

            command.Connection = GetConnection();
            command.SetInterpolatedCommandText(sql);

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a command against the current Database. As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments.
        /// </summary>
        /// <typeparam name="T">The type of the scalar result.</typeparam>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the value of the scalar command.</returns>
        /// <remarks>This method represents a <see cref="NpgsqlCommand.ExecuteScalarAsync(CancellationToken)"/> call.</remarks>
        public async Task<T> ExecuteAsync<T>(string sql, CancellationToken cancellationToken = default) where T : struct
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand(sql, GetConnection());

            return (T)await command.ExecuteScalarAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a command against the current Database. As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments.
        /// </summary>
        /// <typeparam name="T">The type of the scalar result.</typeparam>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="parameters">The SQL Parameters which are being used for the current command.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the value of the scalar command.</returns>
        /// <remarks>This method represents a <see cref="NpgsqlCommand.ExecuteScalarAsync(CancellationToken)"/> call.</remarks>
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

        /// <summary>
        /// Asynchronously executes a command against the current Database. As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional arguments.
        /// </summary>
        /// <typeparam name="T">The type of the scalar result.</typeparam>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="parameters">The SQL Parameters which are being used for the current command.</param>
        /// <returns>A task representing the asynchronous operation, with the value of the scalar command.</returns>
        /// <remarks>This method represents a <see cref="System.Data.Common.DbCommand.ExecuteScalarAsync()"/> call.</remarks>
        public async Task<T> ExecuteAsync<T>(string sql, params NpgsqlParameter[] parameters) where T : struct
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand(sql, GetConnection());

            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.Add(parameters[i]);
            }

            return (T)await command.ExecuteScalarAsync();
        }

        /// <summary>
        /// Asynchronously executes a command against the current Database. This method does automatically parameterize queries from an interpolated string.
        /// </summary>
        /// <typeparam name="T">The type of the scalar result.</typeparam>
        /// <param name="sql">The interpolated SQL to execute.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the value of the scalar command.</returns>
        /// <remarks>This method represents a <see cref="System.Data.Common.DbCommand.ExecuteScalarAsync()"/> call.</remarks>
        public async Task<T> ExecuteInterpolatedAsync<T>(FormattableString sql, CancellationToken cancellationToken = default) where T : struct
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand();

            command.Connection = GetConnection();
            command.SetInterpolatedCommandText(sql);

            var npgsql = new NpgsqlDateTime(DateTime.Now);

            return (T)await command.ExecuteScalarAsync(cancellationToken);
        }

        /// <summary>
        /// Allows for queries against an entity which isn't usually defined, this is usually an entity which hasn't got a table in your database.
        /// </summary>
        /// <returns>A <see cref="TableBase{TEntity}"/> instance from which queries can be executed.</returns>
        /// <remarks>The <typeparamref name="TEntity"/> should always be used with this <see cref="Database"/> instance, otherwise the model has to be generated multiple times.</remarks>
        public TableBase<TEntity> Custom<TEntity>() where TEntity : class, new()
        {
            var entityType = typeof(TEntity);

            if (DatabaseConfigurationCache.CustomEntities.TryGetValue(entityType, out var entity))
            {
                return new TableBase<TEntity>(this, (Entity<TEntity>)entity);
            }

            lock (DatabaseConfigurationCache.CustomEntities)
            {
                if (DatabaseConfigurationCache.CustomEntities.TryGetValue(entityType, out entity))
                {
                    return new TableBase<TEntity>(this, (Entity<TEntity>)entity);
                }

                var entityBuilder = new EntityBuilder<TEntity>(string.Empty);

                entityBuilder.IsRegularEntity = false;

                var entityFactory = new EntityFactory<TEntity>(entityBuilder);

                entity = entityFactory.BuildEntity();

                DatabaseConfigurationCache.CustomEntities.Add(entityType, entity);

                return new TableBase<TEntity>(this, (Entity<TEntity>)entity);
            }
        }

        /// <summary>
        /// Gets or creates a new connections, if none got created yet.
        /// </summary>
        /// <returns>the <see cref="NpgsqlConnection"/>.</returns>
        public NpgsqlConnection GetConnection()
        {
            if (_connection is not null)
                return _connection;

            return _connection = new NpgsqlConnection(ConnectionString);
        }

        internal void ExecuteLoggers(NpgsqlCommand command, Venflow.Enums.CommandType commandType, Exception? exception)
            => ExecuteLoggers(_loggers, command, commandType, exception);

        internal void ExecuteLoggers(IReadOnlyList<LoggerCallback> loggers, NpgsqlCommand command, Venflow.Enums.CommandType commandType, Exception? exception)
        {
            for (int loggerIndex = 0; loggerIndex < loggers.Count; loggerIndex++)
            {
                loggers[loggerIndex].Invoke(command, commandType, exception);
            }
        }

        private void Build()
        {
            var type = this.GetType();

            if (!DatabaseConfigurationCache.DatabaseConfigurations.TryGetValue(type, out var configuration))
            {
                lock (DatabaseConfigurationCache.BuildLocker)
                {
                    if (!DatabaseConfigurationCache.DatabaseConfigurations.TryGetValue(type, out configuration))
                    {
                        var dbConfigurator = new DatabaseConfigurationFactory();

                        var configurationOptionsBuilder = new DatabaseConfigurationOptionsBuilder(type);

                        Configure(configurationOptionsBuilder);

                        configuration = dbConfigurator.BuildConfiguration(type, configurationOptionsBuilder);

                        DatabaseConfigurationCache.DatabaseConfigurations.TryAdd(type, configuration);
                    }
                }
            }

            Entities = configuration.Entities;

            configuration.InstantiateDatabase(this);
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

        /// <summary>
        /// Allows for further configuration of the <see cref="Database"/>.
        /// </summary>
        /// <param name="optionsBuilder">A builder instance used to further configure the <see cref="Database"/>.</param>
        protected virtual void Configure(DatabaseConfigurationOptionsBuilder optionsBuilder) { }

        /// <summary>
        /// Releases the allocated resources for this context. Also closes the underlying connection, if open.
        /// </summary>
        /// <returns>A value task representing the asynchronous operation</returns>
        public ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                return _connection.DisposeAsync();
            }

            if (HasActiveTransaction)
                throw new InvalidOperationException("This database has an open transaction which never has been disposed.");

            return new ValueTask();
        }

        /// <summary>
        /// Releases the allocated resources for this context. Also closes the underlying connection, if open.
        /// </summary>
        /// <remarks>If you are in an asynchronous context you should consider using <see cref="DisposeAsync"/> instead.</remarks>
        public void Dispose()
        {
            if (_connection is not null)
            {
                _connection.Dispose();
            }

            if (HasActiveTransaction)
                throw new InvalidOperationException("This database has an open transaction which never has been disposed.");
        }
    }
}
