using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class JoinOptions
    {
        internal EntityRelation Join { get; }
        internal JoinBehaviour JoinBehaviour { get; }

        internal JoinOptions(EntityRelation joinWith, JoinBehaviour joinBehaviour)
        {
            Join = joinWith;
            JoinBehaviour = joinBehaviour;
        }
    }
}
