using Venflow.Modeling;

namespace Venflow
{
    internal class DatabaseConfiguration
    {
        internal Action<Database, IList<Entity>> DatabaseInstantiater { get; }
        internal IReadOnlyDictionary<string, Entity> Entities { get; }
        internal Dictionary<Type, Entity> CustomEntities { get; }
        internal IList<Entity> EntitiesList { get; }
        internal DatabaseConfigurationOptionsBuilder ConfigurationOptionsBuilder { get; }

        internal DatabaseConfiguration(Action<Database, IList<Entity>> databaseInstantiater, IReadOnlyDictionary<string, Entity> entities, IList<Entity> entitiesList, DatabaseConfigurationOptionsBuilder configurationOptionsBuilder)
        {
            CustomEntities = new Dictionary<Type, Entity>();

            DatabaseInstantiater = databaseInstantiater;
            Entities = entities;
            EntitiesList = entitiesList;
            ConfigurationOptionsBuilder = configurationOptionsBuilder;
        }

        internal void InstantiateDatabase(Database database)
        {
            DatabaseInstantiater.Invoke(database, EntitiesList);
        }
    }
}
