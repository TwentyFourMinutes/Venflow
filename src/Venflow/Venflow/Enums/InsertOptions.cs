using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
