using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Venflow.Dynamic
{
    internal static class TypeFactory
    {
        private static readonly AssemblyName _assemblyName;
        private static readonly AssemblyBuilder _assemblyBuilder;
        private static readonly ModuleBuilder _dynamicModule;

        private static readonly string[] _namespaceNames;

        private static int _typeNumberIdentifier;

        static TypeFactory()
        {
            _assemblyName = new AssemblyName("Venflow.Dynamic");
            _assemblyName.SetPublicKey(typeof(TypeFactory).Assembly.GetName().GetPublicKey());
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
            _dynamicModule = _assemblyBuilder.DefineDynamicModule(_assemblyName.Name + ".dll");

            _namespaceNames = new[] { "Venflow.Dynamic.Proxies.", "Venflow.Dynamic.Materializer.", "Venflow.Dynamic.Inserter." };
        }

        internal static TypeBuilder GetNewProxyBuilder(string typeName, TypeAttributes typeAttributes, Type? parent = null, Type[]? interfaces = null)
        {
            return _dynamicModule.DefineType(GetTypeName(NamespaceType.Proxies, typeName), typeAttributes, parent, interfaces);
        }

        internal static TypeBuilder GetNewMaterializerBuilder(string typeName, TypeAttributes typeAttributes, Type? parent = null, Type[]? interfaces = null)
        {
            return _dynamicModule.DefineType(GetTypeName(NamespaceType.Materializer, typeName + "_" + Interlocked.Increment(ref _typeNumberIdentifier)), typeAttributes, parent, interfaces);
        }

        internal static TypeBuilder GetNewInserterBuilder(string typeName, TypeAttributes typeAttributes, Type? parent = null, Type[]? interfaces = null)
        {
            return _dynamicModule.DefineType(GetTypeName(NamespaceType.Inserter, typeName + "_" + Interlocked.Increment(ref _typeNumberIdentifier)), typeAttributes, parent, interfaces);
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
