using System;

namespace Venflow.Tests.Models
{
    public class UncommonType
    {
        public Guid Id { get; set; }

        public virtual Key<UncommonType, Guid> GuidKey { get; set; }
        public virtual Key<UncommonType, Guid>? NGuidKey { get; set; }

        public virtual Key<UncommonType, ulong> CLRUInt64Key { get; set; }
        public virtual Key<UncommonType, ulong>? NCLRUInt64Key { get; set; }

        public virtual Guid CLRGuid { get; set; }
        public virtual Guid? NCLRGuid { get; set; }

        public virtual DummyEnum CLREnum { get; set; }
        public virtual DummyEnum? NCLREnum { get; set; }

        public virtual PostgreEnum PostgreEnum { get; set; }
        public virtual PostgreEnum? NPostgreEnum { get; set; }

        public virtual ulong CLRUInt64 { get; set; }
        public virtual ulong? NCLRUInt64 { get; set; }
    }
}
