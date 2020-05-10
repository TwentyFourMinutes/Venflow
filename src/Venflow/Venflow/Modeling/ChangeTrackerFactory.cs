using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Venflow.Modeling.ProxyTypes")]

namespace Venflow.Modeling
{
    internal class ChangeTrackerFactory
    {
        private static readonly ModuleBuilder _proxyModule;

        static ChangeTrackerFactory()
        {
            var assemblyName = new AssemblyName("Venflow.Modeling.ProxyTypes");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            _proxyModule = assemblyBuilder.DefineDynamicModule(assemblyName.Name + ".dll");
        }

        internal Func<ChangeTracker<TEntity>, TEntity> GetEntityProxy<TEntity>(Type entityType, Dictionary<int, EntityColumn<TEntity>> properties) where TEntity : class
        {
            var proxyInterfaceType = typeof(IChangeTracking<TEntity>);
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
                // Create Property set property method
                var propertySet = proxyTypeBuilder.DefineMethod("set_" + property.Value.PropertyInfo.Name, MethodAttributes.Private | MethodAttributes.SpecialName |
                                                                                                           MethodAttributes.NewSlot | MethodAttributes.HideBySig |
                                                                                                           MethodAttributes.Virtual | MethodAttributes.Final, null, new[] { property.Value.PropertyInfo.PropertyType });
                var propertySetIL = propertySet.GetILGenerator();

                propertySetIL.Emit(OpCodes.Ldarg_0);
                propertySetIL.Emit(OpCodes.Call, changeTrackerPropertyGet);
                propertySetIL.Emit(OpCodes.Ldc_I4_S, property.Key);
                propertySetIL.Emit(OpCodes.Callvirt, changeTrackerMakeDirtyType);
                propertySetIL.Emit(OpCodes.Ret);

                proxyTypeBuilder.DefineMethodOverride(propertySet, property.Value.PropertyInfo.GetSetMethod()!);
            }

            // Create Constructor
            var constructor = proxyTypeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new[] { changeTrackerType });
            var constructorIL = constructor.GetILGenerator();

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, changeTrackerField);
            constructorIL.Emit(OpCodes.Ret);

            // Create Proxy Type
            var proxyType = proxyTypeBuilder.CreateType();

            // Create the Proxy Type Factory
            var changeTrackerParameter = Expression.Parameter(changeTrackerType, "changeTracker");

            return Expression.Lambda<Func<ChangeTracker<TEntity>, TEntity>>(Expression.Convert(Expression.New(proxyType.GetConstructor(new[] { changeTrackerType }), changeTrackerParameter), typeof(TEntity)), changeTrackerParameter).Compile();
        }
    }

    public static class ChangeTrackingExtensions
    {
        public static TEntity TrackChanges<TEntity>(this TEntity entityInstance, DbConfiguration configuration) where TEntity : class
        {
            if (!configuration.Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.", nameof(TEntity));
            }

            var entity = (Entity<TEntity>)entityModel;

            var changeTracker = new ChangeTracker<TEntity>(entity);

            return entity.ChangeTrackerFactory.Invoke(changeTracker);
        }

        public static void Test<TEntity>(TEntity entity) where TEntity : class
        {
            var columns = ((IChangeTracking<TEntity>)entity).ChangeTracker.GetChangedColumns();
        }
    }

    internal class ChangeTracker<TEntity> where TEntity : class
    {
        private HashSet<int>? _changedColumns;
        private List<EntityColumn<TEntity>>? _changedEntityColumns;

        internal bool IsDirty { get; private set; }

        private readonly Entity<TEntity> _entity;

        internal ChangeTracker(Entity<TEntity> entity)
        {
            _entity = entity;
        }

        internal void MakeDirty(int propertyIndex)
        {
            if (!IsDirty)
            {
                _changedColumns = new HashSet<int>();
                _changedEntityColumns = new List<EntityColumn<TEntity>>();

                IsDirty = true;
            }

            if (!_changedColumns.Contains(propertyIndex))
            {
                _changedEntityColumns.Add(_entity.Columns[propertyIndex]);
            }
        }

        internal List<EntityColumn<TEntity>>? GetChangedColumns()
        {
            return _changedEntityColumns;
        }
    }

    internal interface IChangeTracking<TEntity> where TEntity : class
    {
        ChangeTracker<TEntity> ChangeTracker { get; }
    }
}
