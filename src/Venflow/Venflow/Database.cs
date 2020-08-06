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

    /// <summary>
    /// A <see cref="Database"/> instance represents a session with the database and can be used to perform CRUD operations with your tables and entities.
    /// </summary>
    /// <remarks>
    /// Typically you create a class that derives from <see cref="Database"/> and contains <seealso cref="Table{TEntity}"/> properties for each entity in the Database. All the <seealso cref="Table{TEntity}"/> properties must have a public setter, they are automatically initialized when the instance of the derived type is created.
    /// </remarks>
    public abstract class Database : IAsyncDisposable, IDisposable
    {
        internal IReadOnlyDictionary<string, Entity> Entities { get; private set; }

        internal string ConnectionString { get; }

        private NpgsqlConnection? _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class using the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to your PostgreSQL Database.</param>
        protected Database(string connectionString)
        {
            ConnectionString = connectionString;

            Build();
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
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the value of the scalar command.</returns>
        /// <remarks>This method represents a <see cref="System.Data.Common.DbCommand.ExecuteScalarAsync()"/> call.</remarks>
        public async Task<T> ExecuteInterpolatedAsync<T>(FormattableString sql, CancellationToken cancellationToken = default) where T : struct
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand();

            command.Connection = GetConnection();
            command.SetInterpolatedCommandText(sql);

            return (T)await command.ExecuteScalarAsync(cancellationToken);
        }

        /// <summary>
        /// Gets or creates a new connections, if none got created yet.
        /// </summary>
        /// <returns>the <see cref="NpgsqlConnection"/>.</returns>
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

        /// <summary>
        /// Releases the allocated resources for this context. Also closes the underlying connection, if open.
        /// </summary>
        /// <returns>A value task representing the asynchronous operation</returns>
        public ValueTask DisposeAsync()
        {
            if (_connection is { })
            {
                return _connection.DisposeAsync();
            }

            return new ValueTask();
        }

        /// <summary>
        /// Releases the allocated resources for this context. Also closes the underlying connection, if open.
        /// </summary>
        /// <remarks>If you are in an asynchronous context you should consider using <see cref="DisposeAsync"/> instead.</remarks>
        public void Dispose()
        {
            if (_connection is { })
            {
                _connection.Dispose();
            }
        }
    }
}
