using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Venflow.CodeFirst.Data;
using Venflow.Modeling;

namespace Venflow.CodeFirst
{
    public class MigrationHandler<TDatabase> : IAsyncDisposable, IDisposable
        where TDatabase : Database, new()
    {
        private readonly TDatabase _database;

        public MigrationHandler()
        {
            VenflowConfiguration.PopulateEntityInformation = true;

            _database = new();
        }

        public async Task MigrateAsync()
        {


            var database = new TDatabase();

            var lastMigration = GetLastMigrationAsync(database);

            if (lastMigration is null)
            {


                return;
            }
        }

        private void CreateMigration(Database database)
        {
            var migrationKey = GenerateMigrationKey();

            var migrationGenerator = new MigrationGenerator(migrationKey + "_Initial");

            migrationGenerator.Start();



            migrationGenerator.Finish();
        }

        private async Task EnsureMigrationTableCreated()
        {
            var sql = @"SELECT EXISTS (
   SELECT
   FROM   information_schema.tables
   WHERE  table_schema = 'public' AND table_name = '_VenflowMigrations'
);";
            if (!await _database.ExecuteAsync<bool>(sql))
            {
                var migrationSql = new MigrationTableMigration().ApplyMigration();


            }
        }

        public void Migrate<TDatabase>(TDatabase database) where TDatabase : Database
        {
            var checksum = ComposeChecksum(database);
        }

        private async Task<MigrationDatabaseEntity?> GetLastMigrationAsync(Database database)
        {
            await using var migrationDatabase = new MigrationDatabase(database.ConnectionString);

            var lastMigration = default(MigrationDatabaseEntity);

            lastMigration = await migrationDatabase.Migrations.QuerySingle(@$"SELECT * FROM ""_VenflowMigrations"" ORDER BY ""Timestamp"" DESC LIMIT 1").QueryAsync();

            return lastMigration;
        }

        private string GenerateMigrationKey()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString("X2").ToLower();
        }

        private string ComposeChecksum(Database database)
        {
            var checksumBuilder = new StringBuilder();

            foreach (var entityKeyPair in database.Entities.OrderByDescending(x => x.Value.TableName))
            {
                var entity = entityKeyPair.Value;

                checksumBuilder.Append(entity.TableName);

                var columns = new EntityColumn[entity.GetColumnCount()];

                for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                {
                    columns[columnIndex] = entity.GetColumn(columnIndex);
                }

                foreach (var column in columns.OrderByDescending(x => x.ColumnName))
                {
                    checksumBuilder.Append(column.ColumnName);
                    checksumBuilder.Append(column.PropertyInfo.PropertyType.FullName);
                    checksumBuilder.Append(column.IsNullable);
                }
            }

            return Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(checksumBuilder.ToString())));
        }

        public ValueTask DisposeAsync()
        {
            VenflowConfiguration.PopulateEntityInformation = false;

            return _database.DisposeAsync();
        }

        public void Dispose()
        {
            VenflowConfiguration.PopulateEntityInformation = false;

            _database.Dispose();
        }
    }
}
