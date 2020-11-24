using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Venflow.Design.Data;
using Venflow.Design.Operations;
using Venflow.Modeling;

namespace Venflow.Design
{
    public abstract class MigrationHandler
    {
        protected MigrationHandler()
        {

        }

        internal static string GenerateMigrationKey()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString("X2").ToLower();
        }
    }

    public class MigrationHandler<TDatabase> : MigrationHandler, IAsyncDisposable, IDisposable
        where TDatabase : Database, new()
    {
        private readonly TDatabase _database;
        private readonly MigrationDatabase _migrationDatabase;
        private readonly Assembly _migrationAssembly;

        public MigrationHandler(Assembly migrationAssembly)
        {
            VenflowConfiguration.PopulateEntityInformation = true;
            _database = new();
            _migrationDatabase = new(_database.ConnectionString);

            _migrationDatabase.SetConnection(_database.GetConnection());

            _migrationAssembly = migrationAssembly;
        }

        public bool TryCreateMigration(string migrationName, string? migrationCode)
        {
            if (string.IsNullOrWhiteSpace(migrationName))
                throw new ArgumentException("Can not be null, empty or filled with whitespace.", nameof(migrationName));

            if (char.IsDigit(migrationName[0]))
                throw new ArgumentException("Can not start with a digit.", nameof(migrationName));

            List<IMigrationChange> changes;

            if (!TryGetOldDatabaseSchema(out var migrationContext))
            {
                changes = GetMigrationChanges(new MigrationContext());
            }
            else
            {
                changes = GetMigrationChanges(migrationContext);
            }

            if (changes.Count == 0)
            {
                return false;
            }

            migrationCode = CreateMigrationClass(migrationName, changes);

            return true;
        }

        public async Task<List<Migration>> GetDatabaseMigrationDifferencesAsync()
        {
            var migrationsToApply = new List<Migration>();

            var lastMigration = default(MigrationDatabaseEntity);

            if (await EnsureMigrationTableCreated())
            {
                lastMigration = await GetLastMigrationAsync();
            }

            var currentChecksum = ComposeChecksum();

            if (lastMigration is not null &&
                currentChecksum == lastMigration.Checksum)
            {
                return migrationsToApply;
            }

            if (lastMigration is not null)
            {
                var localMigrations = GetAllLocalMigrations();

                var matchingMigrationFound = false;

                foreach (var localMigration in localMigrations)
                {
                    if (matchingMigrationFound)
                    {
                        migrationsToApply.Add(localMigration);
                    }
                    else if (localMigration.Name == lastMigration.Name)
                    {
                        matchingMigrationFound = true;
                    }
                }
            }
            else
            {
                migrationsToApply.AddRange(GetAllLocalMigrations());
            }

            return migrationsToApply;
        }

        public Task UpdateDatabaseAsync(List<Migration> migrationsToApply)
        {
            return ApplyMigrationsAsync(migrationsToApply);
        }

        private List<IMigrationChange> GetMigrationChanges(MigrationContext migrationContext)
        {
            var migrationChanges = new List<IMigrationChange>();

            foreach (var entityKV in _database.Entities)
            {
                var migrationEntity = new MigrationEntity(entityKV.Value.RawTableName);

                if (migrationContext.Entities.TryGetValue(entityKV.Key, out var oldEntity))
                {
                    // TODO: Add Default value and SQL Comment
                    for (int columnIndex = 0; columnIndex < entityKV.Value.GetColumnCount(); columnIndex++)
                    {
                        var column = entityKV.Value.GetColumn(columnIndex);

                        if (oldEntity.Columns.TryGetValue(column.ColumnName, out var oldColumn))
                        {

                        }
                        else
                        {
                            var nullableColumnType = Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType);

                            var isColumnNullable = nullableColumnType is not null || column.IsNullable || column.IsNullableReferenceType;

                            var columnType = nullableColumnType ?? column.PropertyInfo.PropertyType;

                            columnType = columnType.IsEnum ? Enum.GetUnderlyingType(columnType) : columnType;

                            migrationChanges.Add(new CreateColumnMigration(column.ColumnName, columnType, new ColumnDetails
                            {
                                IsPrimaryKey = column is IPrimaryEntityColumn,
                                IsNullable = isColumnNullable,
                                Precision = column.Information.Precision,
                                Scale = column.Information.Scale

                            }, migrationEntity));
                        }
                    }

                    // TODO: Add Indices/Relation altering
                }
                else
                {
                    migrationChanges.Add(new CreateTableMigration(entityKV.Value.RawTableName, migrationEntity));

                    // TODO: Add Default value and SQL Comment
                    for (int columnIndex = 0; columnIndex < entityKV.Value.GetColumnCount(); columnIndex++)
                    {
                        var column = entityKV.Value.GetColumn(columnIndex);

                        var nullableColumnType = Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType);

                        var isColumnNullable = nullableColumnType is not null || column.IsNullable || column.IsNullableReferenceType;

                        var columnType = nullableColumnType ?? column.PropertyInfo.PropertyType;

                        columnType = columnType.IsEnum ? Enum.GetUnderlyingType(columnType) : columnType;

                        migrationChanges.Add(new CreateColumnMigration(column.ColumnName, columnType ?? column.PropertyInfo.PropertyType, new ColumnDetails
                        {
                            IsPrimaryKey = column is IPrimaryEntityColumn,
                            IsNullable = isColumnNullable,
                            Precision = column.Information.Precision,
                            Scale = column.Information.Scale

                        }, migrationEntity));
                    }

                    if (entityKV.Value.Relations is { })
                    {
                        foreach (var relation in entityKV.Value.Relations.Values)
                        {
                            // TODO: Add relations
                        }
                    }

                    foreach (var index in entityKV.Value.Indices)
                    {
                        // TODO: Add Indices
                    }
                }
            }

            return migrationChanges;
        }

        private bool TryGetOldDatabaseSchema(out MigrationContext migrationContext)
        {
            migrationContext = new MigrationContext();

            var containsMigrations = false;

            foreach (var migration in GetAllLocalMigrations())
            {
                migration.ApplyChanges(migrationContext);

                containsMigrations = true;
            }

            return containsMigrations;
        }

        private IEnumerable<Migration> GetAllLocalMigrations()
        {
            var baseMigrationType = typeof(Migration);

            return _migrationAssembly.GetTypes().Where(x => x.BaseType == baseMigrationType).OrderBy(x =>
            {
                var hexDate = x.Name.Substring(0, x.Name.IndexOf("_"));

                return Convert.ToInt64(hexDate.ToUpper(), 16);
            }).Select(x => (Migration)Activator.CreateInstance(x));
        }

        private Task ApplyVirtualMigrationAsync(Migration migration)
        {
            var migrationSql = new StringBuilder();

            migration.ApplyMigration(migrationSql);

            return _database.ExecuteAsync(migrationSql.ToString());
        }

        private async Task ApplyMigrationsAsync(List<Migration> migrations)
        {
            var migrationSql = new StringBuilder();

            var appliedMigrations = new MigrationDatabaseEntity[migrations.Count];

            var migrationIndex = 0;

            foreach (var migration in migrations)
            {
                migration.ApplyMigration(migrationSql);

                appliedMigrations[migrationIndex++] = new MigrationDatabaseEntity { Name = migration.Name, Checksum = migration.Checksum, Timestamp = DateTime.UtcNow };
            }

            await using var transaction = await _database.BeginTransactionAsync();

            await _database.ExecuteAsync(migrationSql.ToString());

            await transaction.CommitAsync();

            await _migrationDatabase.Migrations.InsertAsync(appliedMigrations);
        }

        private string CreateMigrationClass(string migrationName, List<IMigrationChange> migrationChanges)
        {
            var migrationGenerator = new MigrationGenerator(_migrationAssembly.GetName().Name, migrationName, GenerateMigrationKey() + "_" + migrationName);

            return migrationGenerator.GenerateMigrationClass(migrationChanges);
        }

        private async Task<bool> EnsureMigrationTableCreated()
        {
            var sql = @"SELECT EXISTS (
   SELECT
   FROM   information_schema.tables
   WHERE  table_schema = 'public' AND table_name = '_VenflowMigrations'
);";

            if (!await _database.ExecuteAsync<bool>(sql))
            {
                await ApplyVirtualMigrationAsync(new MigrationTableMigration());

                return false;
            }

            return true;
        }

        private Task<MigrationDatabaseEntity?> GetLastMigrationAsync()
        {
            return _migrationDatabase.Migrations.QuerySingle(@$"SELECT * FROM ""_VenflowMigrations"" ORDER BY ""Timestamp"" DESC LIMIT 1").QueryAsync();
        }

        private string ComposeChecksum()
        {
            var checksumBuilder = new StringBuilder();

            foreach (var entityKeyPair in _database.Entities.OrderByDescending(x => x.Value.RawTableName))
            {
                var entity = entityKeyPair.Value;

                checksumBuilder.Append(entity.RawTableName);

                foreach (var index in entity.Indices)
                {
                    checksumBuilder.Append(index.IndexMethod);
                    checksumBuilder.Append(index.IsConcurrent);
                    checksumBuilder.Append(index.IsUnique);
                    checksumBuilder.Append(index.Name);
                    checksumBuilder.Append(index.NullSortOder);
                    checksumBuilder.Append(index.SortOder);

                    foreach (var property in index.Properties)
                    {
                        checksumBuilder.Append(property.Name);
                    }
                }

                var columns = new EntityColumn[entity.GetColumnCount()];

                for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                {
                    columns[columnIndex] = entity.GetColumn(columnIndex);
                }

                if (entity.Relations is { })
                {
                    foreach (var relation in entity.Relations.Values)
                    {
                        if (relation.LeftNavigationProperty is { })
                        {
                            checksumBuilder.Append(relation.LeftNavigationProperty.Name);
                        }

                        if (relation.RightNavigationProperty is { })
                        {
                            checksumBuilder.Append(relation.RightNavigationProperty.Name);
                        }

                        checksumBuilder.Append(relation.RightEntity.RawTableName);
                        checksumBuilder.Append(relation.IsLeftNavigationPropertyNullable);
                        checksumBuilder.Append(relation.IsRightNavigationPropertyNullable);
                        checksumBuilder.Append(relation.ForeignKeyLocation);
                        checksumBuilder.Append(relation.Information.ConstraintName);
                        checksumBuilder.Append(relation.Information.OnUpdateAction);
                        checksumBuilder.Append(relation.Information.OnDeleteAction);
                    }
                }

                foreach (var column in columns.OrderByDescending(x => x.ColumnName))
                {
                    checksumBuilder.Append(column.ColumnName);
                    checksumBuilder.Append(column.PropertyInfo.PropertyType.FullName);
                    checksumBuilder.Append(column.IsNullable);

                    if (column.Information.DefaultValue is { })
                        checksumBuilder.Append(column.Information.DefaultValue);

                    if (column.Information.Comment is { })
                        checksumBuilder.Append(column.Information.Comment);

                    if (column.Information.Precision.HasValue)
                        checksumBuilder.Append(column.Information.Precision);

                    if (column.Information.Scale.HasValue)
                        checksumBuilder.Append(column.Information.Scale);
                }
            }

            using var sha = SHA1.Create();

            return Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(checksumBuilder.ToString())));
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
