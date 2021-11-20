using System.Data;
using System.Data.Common;
using Npgsql;

namespace Reflow
{
    public class Database<T> : IDatabase where T : Database<T>
    {
        private static readonly DatabaseConfiguration _configuration;

        static Database()
        {
            AssemblyRegister.Assembly ??= typeof(T).Assembly;

            var configurations =
                (Dictionary<Type, DatabaseConfiguration>)AssemblyRegister.Assembly.GetType(
                    "Reflow.DatabaseConfigurations"
                )!.GetField("Configurations")!.GetValue(null)!;

            lock (configurations)
            {
                if (!configurations.Remove(typeof(T), out _configuration!))
                {
                    throw new InvalidOperationException();
                }

#pragma warning disable CS0728 // Possibly incorrect assignment to local which is the argument to a using or lock statement
                if (configurations.Count == 0)
                    configurations = null!;
#pragma warning restore CS0728 // Possibly incorrect assignment to local which is the argument to a using or lock statement
            }
        }

        /// <inheritdoc/>
        public DbConnection? Connection { get; private set; }
        DbConnection? IDatabase.Connection => Connection;

        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;

            _configuration.Instantiater.Invoke(this);
        }

        internal ValueTask EnsureValidConnection(CancellationToken cancellationToken)
        {
            Connection ??= new NpgsqlConnection(_connectionString);

            if (Connection.State == ConnectionState.Open)
            {
                return default;
            }
            else if (Connection.State == ConnectionState.Closed)
            {
                return new ValueTask(Connection.OpenAsync(cancellationToken));
            }
            else
            {
                throw new InvalidOperationException(
                    $"The current connection state is invalid. Expected: '{ConnectionState.Open}' or '{ConnectionState.Closed}'. Actual: '{Connection.State}'."
                );
            }
        }

        ValueTask IDatabase.EnsureValidConnection(CancellationToken cancellationToken) =>
            EnsureValidConnection(cancellationToken);
    }

    internal interface IDatabase
    {
        DbConnection? Connection { get; }
        ValueTask EnsureValidConnection(CancellationToken cancellationToken);
    }
}
