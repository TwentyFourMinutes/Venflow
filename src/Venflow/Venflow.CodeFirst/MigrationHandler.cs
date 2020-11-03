using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Venflow.CodeFirst.Data;
using Venflow.CodeFirst.Operations;
using Venflow.Modeling;

namespace Venflow.CodeFirst
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

        public async Task MigrateAsync()
        {
            var lastMigration = default(MigrationDatabaseEntity);

            var currentChecksum = ComposeChecksum();

            if (await EnsureMigrationTableCreated(currentChecksum))
            {
                lastMigration = await GetLastMigrationAsync();
            }

            List<IMigrationChange> changes;

            if (lastMigration is null)
            {
                changes = GetMigrationChanges(new MigrationContext());

            }
            else if (currentChecksum == lastMigration.Checksum)
                return;
            else
            {
                changes = GetMigrationChanges(GetOldDatabaseSchema());
            }

            if (changes.Count == 0)
                return;

            var migrationClassCreationTask = Task.Run(() => Console.WriteLine(CreateMigrationClass("NotYetDefined", changes)));

            await ApplyMigration("NotYetDefined", changes, currentChecksum);

            await migrationClassCreationTask;
        }

        private List<IMigrationChange> GetMigrationChanges(MigrationContext migrationContext)
        {
            var migrationChanges = new List<IMigrationChange>();

            foreach (var entityKV in _database.Entities)
            {
                var migrationEntity = new MigrationEntity(entityKV.Value.TableName);

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
                    migrationChanges.Add(new CreateTableMigration(entityKV.Value.TableName, migrationEntity));

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

        private MigrationContext GetOldDatabaseSchema()
        {
            var baseMigrationType = typeof(Migration);

            var migrationContext = new MigrationContext();

            foreach (var migrationType in _migrationAssembly.GetTypes().Where(x => x.BaseType == baseMigrationType).OrderBy(x =>
            {
                var hexDate = x.Name[..x.Name.IndexOf("_")];

                return Convert.ToInt64(hexDate.ToUpper(), 16);
            }))
            {
                var migration = (Migration)Activator.CreateInstance(migrationType);

                migration.ApplyChanges(migrationContext);
            }

            return migrationContext;
        }

        private async Task ApplyMigration(Migration migration, string checksum)
        {
            var migrationSql = migration.ApplyMigration();

            await _database.ExecuteAsync(migrationSql.ToString());

            await _migrationDatabase.Migrations.InsertAsync(new MigrationDatabaseEntity { Name = migration.Name, Checksum = checksum, Timestamp = DateTime.UtcNow });
        }

        private async Task ApplyMigration(string migrationName, List<IMigrationChange> migrationChanges, string checksum)
        {
            var migrationSql = new StringBuilder();

            foreach (var migrationChange in migrationChanges)
            {
                migrationChange.ApplyMigration(migrationSql);
            }

            await _database.ExecuteAsync(migrationSql.ToString());

            await _migrationDatabase.Migrations.InsertAsync(new MigrationDatabaseEntity { Name = migrationName, Checksum = checksum, Timestamp = DateTime.UtcNow });
        }

        private string CreateMigrationClass(string migrationName, List<IMigrationChange> migrationChanges)
        {
            var migrationGenerator = new MigrationGenerator(_migrationAssembly.GetName().Name, GenerateMigrationKey() + "_" + migrationName);

            return migrationGenerator.GenerateMigrationClass(migrationChanges);
        }

        private async Task<bool> EnsureMigrationTableCreated(string checksum)
        {
            var sql = @"SELECT EXISTS (
   SELECT
   FROM   information_schema.tables
   WHERE  table_schema = 'public' AND table_name = '_VenflowMigrations'
);";

            if (!await _database.ExecuteAsync<bool>(sql))
            {
                ApplyMigration(new MigrationTableMigration(), checksum);

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
