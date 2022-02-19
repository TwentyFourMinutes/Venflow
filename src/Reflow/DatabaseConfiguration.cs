using System.Reflection;
using Reflow.Lambdas;

namespace Reflow
{
    public class DatabaseConfiguration
    {
        internal Action<object> Instantiater { get; }
        internal IReadOnlyDictionary<Type, Entity> Entities { get; }

        internal IReadOnlyDictionary<Type, Delegate> SingleInserts { get; }
        internal IReadOnlyDictionary<Type, Delegate> ManyInserts { get; }

        internal IReadOnlyDictionary<Type, Delegate> SingleDeletes { get; }
        internal IReadOnlyDictionary<Type, Delegate> ManyDeletes { get; }

        internal LambdaLink[]? LambdaLinks { get; set; }

        internal IReadOnlyDictionary<MethodInfo, ILambdaLinkData> Queries { get; set; } = null!;

        public DatabaseConfiguration(
            Action<object> instantiater,
            IReadOnlyDictionary<Type, Entity> entities,
            LambdaLink[] lambdaLinks,
            IReadOnlyDictionary<Type, Delegate> singleInserts,
            IReadOnlyDictionary<Type, Delegate> manyInserts,
            IReadOnlyDictionary<Type, Delegate> singleDeletes,
            IReadOnlyDictionary<Type, Delegate> manyDeletes
        )
        {
            Instantiater = instantiater;
            Entities = entities;
            LambdaLinks = lambdaLinks;
            SingleInserts = singleInserts;
            ManyInserts = manyInserts;
            SingleDeletes = singleDeletes;
            ManyDeletes = manyDeletes;
        }
    }
}
