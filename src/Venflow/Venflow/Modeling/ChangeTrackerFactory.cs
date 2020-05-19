using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Venflow.Runtime.ProxyTypes")]

namespace Venflow.Modeling
{
    internal static class ChangeTrackerFactory
    {
        internal static ModuleBuilder ProxyModule { get; }

        static ChangeTrackerFactory()
        {
            var assemblyName = new AssemblyName("Venflow.Runtime.ProxyTypes");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            ProxyModule = assemblyBuilder.DefineDynamicModule(assemblyName.Name + ".dll");
        }
    }

    internal class ChangeTrackerFactory<TEntity> where TEntity : class
    {
        internal Type ProxyType { get; private set; }

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

            var changeTrackerMakeDirtyType = _changeTrackerType.GetMethod("MakeDirty", BindingFlags.NonPublic | BindingFlags.Instance)!;

            var proxyTypeBuilder = ChangeTrackerFactory.ProxyModule.DefineType(_entityType.Name, TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit, _entityType, new[] { proxyInterfaceType });

            // Create ChangeTracker backing field
            var changeTrackerField = proxyTypeBuilder.DefineField("_changeTracker", _changeTrackerType, FieldAttributes.Private | FieldAttributes.InitOnly);

            // Create ChangeTracker set property method
            var changeTrackerPropertyGet = proxyTypeBuilder.DefineMethod("get_ChangeTracker", MethodAttributes.Public | MethodAttributes.SpecialName |
                                                                                              MethodAttributes.NewSlot | MethodAttributes.HideBySig |
                                                                                              MethodAttributes.Virtual | MethodAttributes.Final, _changeTrackerType, Type.EmptyTypes);
            var changeTrackerPropertyGetIL = changeTrackerPropertyGet.GetILGenerator();

            changeTrackerPropertyGetIL.Emit(OpCodes.Ldarg_0);
            changeTrackerPropertyGetIL.Emit(OpCodes.Ldfld, changeTrackerField);
            changeTrackerPropertyGetIL.Emit(OpCodes.Ret);

            // Create ChangeTracker property
            var changeTrackerProperty = proxyTypeBuilder.DefineProperty("ChangeTracker", PropertyAttributes.HasDefault, _changeTrackerType, Type.EmptyTypes);
            changeTrackerProperty.SetGetMethod(changeTrackerPropertyGet);

            // Create All Entity properties
            foreach (var property in trackingProperties)
            {
                var baseSetter = property.Value.PropertyInfo.GetSetMethod()!;

                // Create Property set property method
                var propertySet = proxyTypeBuilder.DefineMethod("set_" + property.Value.PropertyInfo.Name, MethodAttributes.Private | MethodAttributes.SpecialName |
                                                                                                           MethodAttributes.NewSlot | MethodAttributes.HideBySig |
                                                                                                           MethodAttributes.Virtual | MethodAttributes.Final, null, new[] { property.Value.PropertyInfo.PropertyType });
                var propertySetIL = propertySet.GetILGenerator();

                propertySetIL.Emit(OpCodes.Ldarg_0);
                propertySetIL.Emit(OpCodes.Ldarg_1);
                propertySetIL.Emit(OpCodes.Call, baseSetter);
                propertySetIL.Emit(OpCodes.Ldarg_0);
                propertySetIL.Emit(OpCodes.Call, changeTrackerPropertyGet);
                propertySetIL.Emit(OpCodes.Ldc_I4_S, property.Key);
                propertySetIL.Emit(OpCodes.Callvirt, changeTrackerMakeDirtyType);
                propertySetIL.Emit(OpCodes.Ret);

                proxyTypeBuilder.DefineMethodOverride(propertySet, baseSetter);
            }

            // Create Constructor
            var constructor = proxyTypeBuilder.DefineConstructor(MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new[] { _changeTrackerType });
            var constructorIL = constructor.GetILGenerator();

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, changeTrackerField);
            constructorIL.Emit(OpCodes.Ret);

            // Create Proxy Type
            ProxyType = proxyTypeBuilder.CreateType();
        }

        internal Func<ChangeTracker<TEntity>, TEntity> GetProxyFactory()
        {
            var changeTrackerParameter = Expression.Parameter(_changeTrackerType, "changeTracker");

            var proxyInstance = Expression.New(ProxyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { _changeTrackerType }, null), changeTrackerParameter);

            return Expression.Lambda<Func<ChangeTracker<TEntity>, TEntity>>(Expression.Convert(proxyInstance, _entityType), changeTrackerParameter).Compile();
        }

        internal Func<ChangeTracker<TEntity>, TEntity, TEntity> GetProxyApplyingFactory(EntityColumnCollection<TEntity> columns)
        {
            var changeTrackerParameter = Expression.Parameter(_changeTrackerType, "changeTracker");
            var entityParameter = Expression.Parameter(_entityType, "entity");

            var bindings = new MemberBinding[columns.Count];

            int index = 0;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                bindings[index++] = Expression.Bind(ProxyType.GetProperty(column.PropertyInfo.Name), Expression.Property(entityParameter, column.PropertyInfo.Name));
            }

            var proxyVariable = Expression.Variable(ProxyType, "proxy");

            var proxyInstance = Expression.New(ProxyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { _changeTrackerType }, null), changeTrackerParameter);

            var block = Expression.Block(_entityType, new[] { proxyVariable },
                                         Expression.Assign(proxyVariable, Expression.MemberInit(proxyInstance, bindings)),
                                         Expression.Assign(Expression.Property(Expression.Property(proxyVariable, "ChangeTracker"), _changeTrackerType.GetProperty("TrackChanges", BindingFlags.NonPublic | BindingFlags.Instance)), ExpressionCache.TrueConstant),
                                         Expression.Convert(proxyVariable, _entityType));

            return Expression.Lambda<Func<ChangeTracker<TEntity>, TEntity, TEntity>>(block, changeTrackerParameter, entityParameter).Compile();
        }
    }
}
