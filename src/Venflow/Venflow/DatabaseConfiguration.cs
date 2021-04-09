using System;
using System.Collections.Generic;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow
{
    internal class DatabaseConfiguration
    {
        internal Action<Database, IList<Entity>> DatabaseInstantiater { get; }
        internal IReadOnlyDictionary<string, Entity> Entities { get; }
        internal Dictionary<Type, Entity> CustomEntities { get; }
        internal IList<Entity> EntitiesList { get; }

        internal IReadOnlyList<LoggerCallback> Loggers { get; }
        internal LoggingBehavior DefaultLoggingBehavior { get; }

        internal DatabaseConfiguration(Action<Database, IList<Entity>> databaseInstantiater, IReadOnlyDictionary<string, Entity> entities, IList<Entity> entitiesList, IReadOnlyList<LoggerCallback> loggers, LoggingBehavior defaultLoggingBehavior)
        {
            CustomEntities = new Dictionary<Type, Entity>();

            DatabaseInstantiater = databaseInstantiater;
            Entities = entities;
            EntitiesList = entitiesList;
            Loggers = loggers;
            DefaultLoggingBehavior = defaultLoggingBehavior;
        }

        internal void InstantiateDatabase(Database database)
        {
            DatabaseInstantiater.Invoke(database, EntitiesList);
        }
    }
}
