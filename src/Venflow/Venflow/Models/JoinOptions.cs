using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Models
{
    internal class JoinOptions
    {
        internal ForeignEntity JoinWith { get; }
        internal JoinBehaviour JoinBehaviour { get; }

        internal JoinOptions(ForeignEntity joinWith, JoinBehaviour joinBehaviour)
        {
            JoinWith = joinWith;
            JoinBehaviour = joinBehaviour;
        }
    }
}
