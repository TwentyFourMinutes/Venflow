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

            var entitiesIndexerMethod = entitiesListType.GetMethod("get_Item")!;

            var instantiaterMethod = TypeFactory.GetDynamicMethod($"Venflow.Dynamic.Instantiater.{customDatabaseType.Name}Instantiater", null, new[] { databaseType, entitiesListType });

            var instantiaterMethodIL = instantiaterMethod.GetILGenerator();

            for (var i = 0; i < tableProperties.Count; i++)
            {
                var tableProperty = tableProperties[i];

                // Instantiate table property
                instantiaterMethodIL.Emit(OpCodes.Ldarg_0);
                instantiaterMethodIL.Emit(OpCodes.Ldarg_0);
                instantiaterMethodIL.Emit(OpCodes.Ldarg_1);
                instantiaterMethodIL.Emit(OpCodes.Ldc_I4, i);
                instantiaterMethodIL.Emit(OpCodes.Callvirt, entitiesIndexerMethod);
                instantiaterMethodIL.Emit(OpCodes.Newobj, tableProperty.PropertyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { databaseType, genericEntityType.MakeGenericType(entities[i].EntityType) }, null)!);
                instantiaterMethodIL.Emit(OpCodes.Callvirt, tableProperty.GetSetMethod()!);
            }

            instantiaterMethodIL.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return instantiaterMethod.CreateDelegate<Action<Database, IList<Entity>>>();
#else
            return (Action<Database, IList<Entity>>)instantiaterMethod.CreateDelegate(typeof(Action<Database, IList<Entity>>));
#endif
        }
    }
}
