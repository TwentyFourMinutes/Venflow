using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow
{
    public abstract class DbConfiguration
    {
        internal string ConnectionString { get; }
        internal bool UseLazyEntityEvaluation { get; }

        internal IReadOnlyDictionary<string, IEntity> Entities { get; private set; }
        internal bool IsBuild { get; private set; }

        protected DbConfiguration(string connectionString, bool useLazyEntityEvaluation = false)
        {
            ConnectionString = connectionString;
            UseLazyEntityEvaluation = useLazyEntityEvaluation;
            Entities = null!;
        }

        public async ValueTask<VenflowDbConnection> NewConnectionScopeAsync(bool openConnection = true, CancellationToken cancellationToken = default)
        {
            if (!this.IsBuild)
                Build();

            var connection = new NpgsqlConnection(ConnectionString);

            if (openConnection)
            {
                await connection.OpenAsync(cancellationToken);
            }

            return new VenflowDbConnection(this, connection);
        }

        protected abstract void Configure(DbConfigurator dbConfigurator);

        internal void Build()
        {
            if (IsBuild)
                return;

            var dbConfigurator = new DbConfigurator();

            Configure(dbConfigurator);

            Entities = dbConfigurator.BuildConfiguration();

            IsBuild = true;
        }
    }
}
