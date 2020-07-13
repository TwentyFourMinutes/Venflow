using System;
using System.Collections.Generic;
using Venflow.Modeling;

namespace Venflow
{
    internal class DatabaseConfiguration
    {
        internal Action<Database, IList<Entity>> DatabaseInstantiater { get; }
        internal IReadOnlyDictionary<string, Entity> Entities { get; }
        internal IList<Entity> EntitiesList { get; }

        internal DatabaseConfiguration(Action<Database, IList<Entity>> databaseInstantiater, IReadOnlyDictionary<string, Entity> entities, IList<Entity> entitiesList)
        {
            DatabaseInstantiater = databaseInstantiater;
            Entities = entities;
            EntitiesList = entitiesList;
        }

        internal void InstantiateDatabase(Database database)
        {
            DatabaseInstantiater.Invoke(database, EntitiesList);
        }
    }
}
