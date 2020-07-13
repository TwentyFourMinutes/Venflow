using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class JoinOptions
    {
        internal EntityRelation JoinWith { get; }
        internal Entity JoinFrom { get; }
        internal JoinBehaviour JoinBehaviour { get; }

        internal JoinOptions(EntityRelation joinWith, Entity joinFrom, JoinBehaviour joinBehaviour)
        {
            JoinWith = joinWith;
            JoinFrom = joinFrom;
            JoinBehaviour = joinBehaviour;
        }
    }
}
