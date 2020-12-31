using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Venflow.Modeling;

namespace Venflow.Dynamic.Instantiater
{
    internal static class DatabaseTableFactory
    {
        internal static Action<Database, IList<Entity>> CreateInstantiater(Type customDatabaseType, IList<PropertyInfo> tableProperties, IList<Entity> entities)
        {
            var databaseType = typeof(Database);
            var entitiesListType = typeof(IList<Entity>);

            var genericEntityType = typeof(Entity<>);

            var entitiesIndexerMethod = entitiesListType.GetMethod("get_Item");

            var instantiaterMethod = new DynamicMethod($"Venflow.Dynamic.Instantiater.{customDatabaseType.Name}Instantiater", null, new[] { databaseType, entitiesListType }, TypeFactory.DynamicModule);

            var instantiaterMethodIL = instantiaterMethod.GetILGenerator();

            for (int i = 0; i < tableProperties.Count; i++)
            {
                var tableProperty = tableProperties[i];

                // Instantiate table property
                instantiaterMethodIL.Emit(OpCodes.Ldarg_0);
                instantiaterMethodIL.Emit(OpCodes.Ldarg_0);
                instantiaterMethodIL.Emit(OpCodes.Ldarg_1);
                instantiaterMethodIL.Emit(OpCodes.Ldc_I4, i);
                instantiaterMethodIL.Emit(OpCodes.Callvirt, entitiesIndexerMethod);
                instantiaterMethodIL.Emit(OpCodes.Newobj, tableProperty.PropertyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { databaseType, genericEntityType.MakeGenericType(entities[i].EntityType) }, null));
                instantiaterMethodIL.Emit(OpCodes.Callvirt, tableProperty.GetSetMethod());
            }

            instantiaterMethodIL.Emit(OpCodes.Ret);

#if NETCOREAPP5_0
            return instantiaterMethod.CreateDelegate<Action<Database, IList<Entity>>>();
#else
            return (Action<Database, IList<Entity>>)instantiaterMethod.CreateDelegate(typeof(Action<Database, IList<Entity>>));
#endif
        }
    }
}
