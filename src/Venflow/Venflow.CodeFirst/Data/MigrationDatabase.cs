namespace Venflow.CodeFirst.Data
{

    internal class MigrationDatabase : Database
    {
        public Table<MigrationDatabaseEntity> Migrations { get; set; }

        public MigrationDatabase(string connectionString) : base(connectionString)
        {

        }
    }
}
