using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.CodeFirst
{

    public class MigrationHandler
    {
        public async Task MigrateAsync<TDatabase>() where TDatabase : Database, new()
        {
            VenflowConfiguration.PopulateColumnInformation = true;

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

        public void Migrate<TDatabase>(TDatabase database) where TDatabase : Database
        {
            var checksum = ComposeChecksum(database);
        }

        private async Task<MigrationDatabaseEntity?> GetLastMigrationAsync(Database database)
        {
            await using var migrationDatabase = new MigrationDatabase(database.ConnectionString);

            var lastMigration = default(MigrationDatabaseEntity);

            try
            {
                lastMigration = await migrationDatabase.Migrations.QuerySingle(@$"SELECT * FROM ""_VenflowMigrations"" ORDER BY ""Timestamp"" DESC LIMIT 1").QueryAsync();
            }
            catch (Npgsql.PostgresException exception) when (exception.Message == @"42P01: relation ""_VenflowMigrations"" does not exist")
            {
                await migrationDatabase.ExecuteAsync(@"CREATE TABLE ""_VenflowMigrations""");
            }

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
    }
}
