using System;

namespace Venflow.Enums
{
    [Flags]
    public enum InsertOptions
    {
        None = 0,
        PopulateRelations = 1,
        SetIdentityColumns = 2 | PopulateRelations
    }
}
