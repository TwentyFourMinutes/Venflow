using System.Reflection;

namespace Reflow.Lambdas
{
    internal static class LambdaLinker
    {
        private static readonly IReadOnlyDictionary<MethodInfo, ILambdaLinkData> _lambdaData;

        static LambdaLinker()
        {
            if (AssemblyRegister.Assembly is null)
                throw new InvalidOperationException();

            var linksField = AssemblyRegister.Assembly.GetType(
                "Reflow.Lambdas.LambdaLinks"
            )!.GetField("Links")!;

            var links = (LambdaLink[])linksField.GetValue(null)!;

            var lambdaData = new Dictionary<MethodInfo, ILambdaLinkData>(links.Length);

            for (var linkIndex = 0; linkIndex < links.Length; linkIndex++)
            {
                var link = links[linkIndex];

                MethodInfo? method = null;

                if (link.HasClosure)
                {
                    var nestedTypes = link.ClassType.GetNestedTypes(BindingFlags.NonPublic);

                    var expectedTypeName = "<>c__DisplayClass" + link.MemberIndex;

                    for (
                        var nestedTypeIndex = 0;
                        nestedTypeIndex < nestedTypes.Length;
                        nestedTypeIndex++
                    )
                    {
                        var nestedType = nestedTypes[nestedTypeIndex];

                        if (!nestedType.Name.StartsWith(expectedTypeName))
                            continue;

                        var tempMethod = nestedType.GetMethod(
                            $"<{link.IdentifierName}>b__{link.LambdaIndex}",
                            BindingFlags.NonPublic | BindingFlags.Instance
                        );

                        if (tempMethod is null)
                            continue;

                        method = tempMethod;
                    }

                    if (method is null)
                        throw new InvalidOperationException();
                }
                else
                {
                    var nestedType = link.ClassType.GetNestedType("<>c", BindingFlags.NonPublic)!;

                    method =
                        nestedType.GetMethod(
                            $"<{link.IdentifierName}>b__{link.MemberIndex}_{link.LambdaIndex}",
                            BindingFlags.NonPublic | BindingFlags.Instance
                        ) ?? throw new InvalidOperationException();
                }

                lambdaData.Add(method, link.Data);
            }

            _lambdaData = lambdaData;

            linksField.SetValue(null, null);
        }

        internal static TData GetLambdaData<TData>(MethodInfo method) where TData : ILambdaLinkData
        {
            return (TData)_lambdaData[method];
        }
    }
}
