using System.Linq.Expressions;
using System.Reflection.Emit;
using Venflow.Modeling;

namespace Venflow.Dynamic.Proxies
{
    internal class ChangeTrackerFactory<TEntity> where TEntity : class, new()
    {
        internal Type ProxyType { get; private set; } = null!;

        private readonly Type _entityType;
        private readonly Type _changeTrackerType;

        internal ChangeTrackerFactory(Type entityType)
        {
            _entityType = entityType;
            _changeTrackerType = typeof(ChangeTracker<TEntity>);
        }

        internal void GenerateEntityProxy(Dictionary<int, EntityColumn<TEntity>> trackingProperties)
        {
            var proxyInterfaceType = typeof(IEntityProxy<TEntity>);

            var changeTrackerMakeDirtyMethod = _changeTrackerType.GetMethod("MakeDirty", BindingFlags.NonPublic | BindingFlags.Instance)!;

            var proxyTypeBuilder = TypeFactory.GetNewProxyBuilder(_entityType.Name, TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit, _entityType, new[] { proxyInterfaceType });

            // Create ChangeTracker backing field
            var changeTrackerField = proxyTypeBuilder.DefineField("_changeTracker", _changeTrackerType, FieldAttributes.Private | FieldAttributes.InitOnly);

            // Create ChangeTracker set property method
            var changeTrackerPropertyGet = proxyTypeBuilder.DefineMethod("get_ChangeTracker", MethodAttributes.Public | MethodAttributes.SpecialName |
                                                                                              MethodAttributes.NewSlot | MethodAttributes.HideBySig |
                                                                                              MethodAttributes.Virtual | MethodAttributes.Final, _changeTrackerType, Type.EmptyTypes);
            changeTrackerPropertyGet.InitLocals = false;

            var changeTrackerPropertyGetIL = changeTrackerPropertyGet.GetILGenerator();

            changeTrackerPropertyGetIL.Emit(OpCodes.Ldarg_0);
            changeTrackerPropertyGetIL.Emit(OpCodes.Ldfld, changeTrackerField);
            changeTrackerPropertyGetIL.Emit(OpCodes.Ret);

            // Create ChangeTracker property
            var changeTrackerProperty = proxyTypeBuilder.DefineProperty("ChangeTracker", PropertyAttributes.HasDefault, _changeTrackerType, Type.EmptyTypes);
            changeTrackerProperty.SetGetMethod(changeTrackerPropertyGet);

            var propertyIndex = 0;

            // Create All Entity properties
            foreach (var property in trackingProperties)
            {
                var baseSetter = property.Value.PropertyInfo.GetSetMethod()!;

                // Create Property set property method
                var propertySet = proxyTypeBuilder.DefineMethod("set_" + property.Value.PropertyInfo.Name, MethodAttributes.Public | MethodAttributes.SpecialName |
                                                                                                           MethodAttributes.Virtual | MethodAttributes.HideBySig, null, new[] { property.Value.PropertyInfo.PropertyType });
                propertySet.InitLocals = false;

                var propertySetIL = propertySet.GetILGenerator();

                propertySetIL.Emit(OpCodes.Ldarg_0);
                propertySetIL.Emit(OpCodes.Ldarg_1);
                propertySetIL.Emit(OpCodes.Call, baseSetter);
                propertySetIL.Emit(OpCodes.Ldarg_0);
                propertySetIL.Emit(OpCodes.Call, changeTrackerPropertyGet);
                propertySetIL.Emit(OpCodes.Ldc_I4_S, (byte)propertyIndex++);
                propertySetIL.Emit(OpCodes.Ldc_I4_S, (byte)property.Key);
                propertySetIL.Emit(OpCodes.Callvirt, changeTrackerMakeDirtyMethod);
                propertySetIL.Emit(OpCodes.Ret);

                proxyTypeBuilder.DefineMethodOverride(propertySet, baseSetter);
            }

            // Create Constructor
            var constructor = proxyTypeBuilder.DefineConstructor(MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new[] { _changeTrackerType });
            var constructorIL = constructor.GetILGenerator();

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Call, _entityType.GetConstructor(Type.EmptyTypes)!);
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, changeTrackerField);
            constructorIL.Emit(OpCodes.Ret);

            // Create Proxy Type
            ProxyType = proxyTypeBuilder.CreateType()!;
        }

        internal Func<ChangeTracker<TEntity>, TEntity> GetProxyFactory()
        {
            var changeTrackerParameter = Expression.Parameter(_changeTrackerType, "changeTracker");

            var proxyInstance = Expression.New(ProxyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { _changeTrackerType }, null)!, changeTrackerParameter);

            return Expression.Lambda<Func<ChangeTracker<TEntity>, TEntity>>(Expression.Convert(proxyInstance, _entityType), changeTrackerParameter).Compile();
        }

        internal Func<ChangeTracker<TEntity>, TEntity, TEntity> GetProxyApplyingFactory()
        {
            var method = TypeFactory.GetDynamicMethod(_entityType.Name + "ProxyApplier", typeof(TEntity), new[] { _changeTrackerType, typeof(TEntity) });
            var ilGenerator = method.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Newobj, ProxyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { _changeTrackerType }, null)!);

            var properties = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).AsSpan();

            for (var propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
            {
                var property = properties[propertyIndex];

                var backingField = property.GetBackingField();

                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.Emit(OpCodes.Ldarg_1);

                if (!property.CanWrite &&
                    backingField is not null)
                {
                    ilGenerator.Emit(OpCodes.Ldfld, backingField);
                    ilGenerator.Emit(OpCodes.Stfld, backingField);
                }
                else
                {
                    var proxyProperty = ProxyType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) ?? property;

                    ilGenerator.Emit(OpCodes.Callvirt, property.GetGetMethod()!);

                    ilGenerator.Emit(OpCodes.Callvirt, proxyProperty.GetSetMethod(true)!);
                }
            }

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldc_I4_1);
            ilGenerator.Emit(OpCodes.Callvirt, _changeTrackerType.GetProperty("TrackChanges", BindingFlags.NonPublic | BindingFlags.Instance)!.GetSetMethod(true)!);
            ilGenerator.Emit(OpCodes.Ret);

            return (Func<ChangeTracker<TEntity>, TEntity, TEntity>)method.CreateDelegate(typeof(Func<ChangeTracker<TEntity>, TEntity, TEntity>));
        }
    }
}
