using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Venflow.Runtime.ProxyTypes")]

namespace Venflow.Modeling
{
    internal class ChangeTrackerFactory
    {
        private static readonly ModuleBuilder _proxyModule;

        static ChangeTrackerFactory()
        {
            var assemblyName = new AssemblyName("Venflow.Runtime.ProxyTypes");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            _proxyModule = assemblyBuilder.DefineDynamicModule(assemblyName.Name + ".dll");
        }

        internal (Type proxyType, Func<ChangeTracker<TEntity>, TEntity> factory, Func<ChangeTracker<TEntity>, TEntity, TEntity> applier) GenerateEntityProxyFactories<TEntity>(Type entityType, Dictionary<int, EntityColumn<TEntity>> properties) where TEntity : class
        {
            var proxyInterfaceType = typeof(IEntityProxy<TEntity>);
            var changeTrackerType = typeof(ChangeTracker<TEntity>);
            var changeTrackerMakeDirtyType = changeTrackerType.GetMethod("MakeDirty", BindingFlags.NonPublic | BindingFlags.Instance)!;

            var proxyTypeBuilder = _proxyModule.DefineType(entityType.Name, TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit, entityType, new[] { proxyInterfaceType });

            // Create ChangeTracker backing field
            var changeTrackerField = proxyTypeBuilder.DefineField("_changeTracker", changeTrackerType, FieldAttributes.Private | FieldAttributes.InitOnly);

            // Create ChangeTracker set property method
            var changeTrackerPropertyGet = proxyTypeBuilder.DefineMethod("get_ChangeTracker", MethodAttributes.Public | MethodAttributes.SpecialName |
                                                                                              MethodAttributes.NewSlot | MethodAttributes.HideBySig |
                                                                                              MethodAttributes.Virtual | MethodAttributes.Final, changeTrackerType, Type.EmptyTypes);
            var changeTrackerPropertyGetIL = changeTrackerPropertyGet.GetILGenerator();

            changeTrackerPropertyGetIL.Emit(OpCodes.Ldarg_0);
            changeTrackerPropertyGetIL.Emit(OpCodes.Ldfld, changeTrackerField);
            changeTrackerPropertyGetIL.Emit(OpCodes.Ret);

            // Create ChangeTracker property
            var changeTrackerProperty = proxyTypeBuilder.DefineProperty("ChangeTracker", PropertyAttributes.HasDefault, changeTrackerType, Type.EmptyTypes);
            changeTrackerProperty.SetGetMethod(changeTrackerPropertyGet);

            // Create All Entity properties
            foreach (var property in properties)
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
            var constructor = proxyTypeBuilder.DefineConstructor(MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new[] { changeTrackerType });
            var constructorIL = constructor.GetILGenerator();

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, changeTrackerField);
            constructorIL.Emit(OpCodes.Ret);

            // Create Proxy Type
            var proxyType = proxyTypeBuilder.CreateType();

            var changeTrackerParameter = Expression.Parameter(changeTrackerType, "changeTracker");
            var entityParameter = Expression.Parameter(entityType, "entity");

            var proxyInstance = Expression.New(proxyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { changeTrackerType }, null), changeTrackerParameter);

            var factory = Expression.Lambda<Func<ChangeTracker<TEntity>, TEntity>>(Expression.Convert(proxyInstance, entityType), changeTrackerParameter).Compile();

            var bindings = new MemberBinding[properties.Count];

            int index = 0;

            foreach (var property in properties.Values)
            {
                bindings[index++] = Expression.Bind(proxyType.GetProperty(property.PropertyInfo.Name), Expression.Property(entityParameter, property.PropertyInfo.Name));
            }

            var proxyVariable = Expression.Variable(proxyType, "proxy");

            var block = Expression.Block(entityType, new[] { proxyVariable },
                                         Expression.Assign(proxyVariable, Expression.MemberInit(proxyInstance, bindings)),
                                         Expression.Assign(Expression.Property(Expression.Property(proxyVariable, "ChangeTracker"), changeTrackerType.GetProperty("TrackChanges", BindingFlags.NonPublic | BindingFlags.Instance)), Expression.Constant(true)),
                                         Expression.Convert(proxyVariable, entityType));

            var applier = Expression.Lambda<Func<ChangeTracker<TEntity>, TEntity, TEntity>>(block, changeTrackerParameter, entityParameter).Compile();

            return (proxyType, factory, applier);
        }
    }
}
