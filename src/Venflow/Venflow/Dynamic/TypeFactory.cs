using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Venflow.Dynamic
{
    internal static class TypeFactory
    {
        internal static Module DynamicModule => _dynamicModule;

        private static readonly AssemblyName _assemblyName;
        private static readonly AssemblyBuilder _assemblyBuilder;
        private static readonly ModuleBuilder _dynamicModule;

        private static readonly HashSet<string> _knownEntityAssemblies;

        private static readonly string[] _namespaceNames;

        private static int _typeNumberIdentifier;

        static TypeFactory()
        {
            _assemblyName = new AssemblyName("Venflow.Dynamic");
            _assemblyName.SetPublicKey(typeof(TypeFactory).Assembly.GetName().GetPublicKey());
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
            _dynamicModule = _assemblyBuilder.DefineDynamicModule(_assemblyName.Name + ".dll");

#if NET5_0_OR_GREATER
            _dynamicModule.SetCustomAttribute(new CustomAttributeBuilder(typeof(SkipLocalsInitAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>()));
#endif

            _namespaceNames = new[] { "Venflow.Dynamic.Proxies.", "Venflow.Dynamic.Materializer.", "Venflow.Dynamic.Inserter." };

            _knownEntityAssemblies = new(1);
        }

        internal static void AddEntityAssembly(string assemblyName)
        {
            if (!_knownEntityAssemblies.Add(assemblyName))
                return;

            var ignoresAccessChecksTo = new CustomAttributeBuilder
            (
                typeof(IgnoresAccessChecksToAttribute).GetConstructor(new Type[] { typeof(string) }),
                new object[] { assemblyName }
            );

            _assemblyBuilder.SetCustomAttribute(ignoresAccessChecksTo);
        }

        internal static TypeBuilder GetNewProxyBuilder(string typeName, TypeAttributes typeAttributes, Type? parent = null, Type[]? interfaces = null)
        {
            return _dynamicModule.DefineType(GetTypeName(NamespaceType.Proxies, typeName + "_" + Interlocked.Increment(ref _typeNumberIdentifier)), typeAttributes, parent, interfaces);
        }

        internal static TypeBuilder GetNewMaterializerBuilder(string typeName, TypeAttributes typeAttributes, Type? parent = null, Type[]? interfaces = null)
        {
            return _dynamicModule.DefineType(GetTypeName(NamespaceType.Materializer, typeName + "_" + Interlocked.Increment(ref _typeNumberIdentifier)), typeAttributes, parent, interfaces);
        }

        internal static TypeBuilder GetNewInserterBuilder(string typeName, TypeAttributes typeAttributes, Type? parent = null, Type[]? interfaces = null)
        {
            return _dynamicModule.DefineType(GetTypeName(NamespaceType.Inserter, typeName + "_" + Interlocked.Increment(ref _typeNumberIdentifier)), typeAttributes, parent, interfaces);
        }

        internal static DynamicMethod GetDynamicMethod(string methodName, Type? returnType, Type[]? parameters, bool skipVisiblity = true)
        {
            var method = new DynamicMethod(methodName + "_" + Interlocked.Increment(ref _typeNumberIdentifier), returnType, parameters, DynamicModule, skipVisiblity);

            method.InitLocals = false;

            return method;
        }

        private static string GetTypeName(NamespaceType namespaceType, string typeName)
        {
            return _namespaceNames[(int)namespaceType] + typeName;
        }

        private enum NamespaceType
        {
            Proxies = 0,
            Materializer = 1,
            Inserter = 2
        }
    }
}
