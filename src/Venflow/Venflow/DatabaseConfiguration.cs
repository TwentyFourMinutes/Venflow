using System;
using System.Collections.Generic;
using Npgsql;
using Venflow.Modeling;

namespace Venflow
{
    internal class DatabaseConfiguration
    {
        internal Action<Database, IList<Entity>> DatabaseInstantiater { get; }
        internal IReadOnlyDictionary<string, Entity> Entities { get; }
        internal Dictionary<Type, Entity> CustomEntities { get; }
        internal IList<Entity> EntitiesList { get; }
        public INpgsqlNameTranslator NpgsqlNameTranslator { get; }

        internal DatabaseConfiguration(
            Action<Database, IList<Entity>> databaseInstantiater,
            IReadOnlyDictionary<string, Entity> entities,
            IList<Entity> entitiesList,
            INpgsqlNameTranslator nameTranslator
            )
        {
            CustomEntities = new Dictionary<Type, Entity>();

            DatabaseInstantiater = databaseInstantiater;
            Entities = entities;
            EntitiesList = entitiesList;
            NpgsqlNameTranslator = nameTranslator;
        }

        internal void InstantiateDatabase(Database database)
        {
            DatabaseInstantiater.Invoke(database, EntitiesList);
        }
    }
}
