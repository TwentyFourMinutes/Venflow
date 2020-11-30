using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Venflow.Tools.CLI
{
    internal class MigrationHandler
    {
        private readonly Type _migrationType;
        private readonly object _migrationHandlerInstance;

        internal MigrationHandler(Assembly designAssemblyPath, Assembly migrationAssembly)
        {
            _migrationType = designAssemblyPath.GetTypes().First(x => x.Name == "MigrationHandler");

            _migrationHandlerInstance = _migrationType.GetMethod("GetMigrationHandler", 0, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Assembly) }, null).Invoke(null, new object[] { migrationAssembly });
        }

        internal MigrationHandler(Assembly designAssemblyPath, Assembly migrationAssembly, string databaseTypeName)
        {
            _migrationType = designAssemblyPath.GetTypes().First(x => x.Name == "MigrationHandler");

            _migrationHandlerInstance = _migrationType.GetMethod("GetMigrationHandler", 0, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Assembly), typeof(string) }, null).Invoke(null, new object[] { migrationAssembly, databaseTypeName });
        }


        internal bool TryCreateMigration(string migrationName, out string? migrationCode)
        {
            var args = new object[] { migrationName, string.Empty };

            var result = (bool)_migrationType.GetMethod("TryCreateMigration").Invoke(_migrationHandlerInstance, args);

            migrationCode = (string)args[1];

            return result;
        }

        internal async Task<ICollection> GetDatabaseMigrationDifferencesAsync()
        {
            var method = _migrationType.GetMethod("GetDatabaseMigrationDifferencesAsync");

            var originalTask = method.Invoke(_migrationHandlerInstance, null);

            await (Task)originalTask;

            return (ICollection)method.ReturnType.GetProperty("Result").GetGetMethod().Invoke(originalTask, null);
        }

        internal Task UpdateDatabaseAsync(ICollection migrationsToApply)
        {
            return (Task)_migrationType.GetMethod("UpdateDatabaseAsync").Invoke(_migrationHandlerInstance, new object[] { migrationsToApply });
        }
    }
}
