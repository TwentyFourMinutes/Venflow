using System;

namespace Venflow
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratedKeyAttribute : Attribute
    {
        private Type _keyType;

        public GeneratedKeyAttribute(Type keyType)
        {
            _keyType = keyType;
        }
    }
}