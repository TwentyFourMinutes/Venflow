using System;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Shared;
using Venflow.Tests.Models;

namespace Venflow.Tests
{
    public abstract class TestBase : IAsyncDisposable
    {
        private static bool _isSetupDone;
        private static object _setupLock = new object();
        private static string _connectionString;

        protected RelationDatabase Database { get; }

        protected TestBase()
        {
            ApplySetup();

            Database = new RelationDatabase(_connectionString);
        }

        private static void ApplySetup()
        {
            if (_isSetupDone)
                return;

            lock (_setupLock)
            {
                if (_isSetupDone)
                    return;

                ApplySetupAsync().GetAwaiter().GetResult();

                _isSetupDone = true;
            }
        }

        private static async Task ApplySetupAsync()
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(SecretsHandler.GetConnectionString<RelationDatabase>());

            connectionStringBuilder.Database = "venflow_unit_tests";

            _connectionString = connectionStringBuilder.ToString();

            connectionStringBuilder.Database = "postgres";

            await using var db = new PostgresDatabase(connectionStringBuilder.ToString());

            const string checkForDbSql = @"select exists(
    SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('venflow_unit_tests')
);";

            if (await db.ExecuteAsync<bool>(checkForDbSql))
            {
                await db.ExecuteAsync("DROP DATABASE venflow_unit_tests");
            }


        }

        public ValueTask DisposeAsync()
        {
            return Database.DisposeAsync();
        }

        private class PostgresDatabase : Database
        {
            internal PostgresDatabase(string connectionString) : base(connectionString)
            {

            }
        }
    }
}
