using System.Reflection;

namespace Reflow.Lambdas
{
    internal static class LambdaLinker
    {
        private static readonly IReadOnlyDictionary<MethodInfo, LambdaData> _lambdaData;

        static LambdaLinker()
        {
            if (AssemblyRegister.Assembly is null)
                throw new InvalidOperationException();

            var linksField = AssemblyRegister.Assembly.GetType(
                "Reflow.Lambdas.LambdaLinks"
            )!.GetField("Links")!;

            var links = (LambdaLink[])linksField.GetValue(null)!;

            var lambdaData = new Dictionary<MethodInfo, LambdaData>(links.Length);

            for (var linkIndex = 0; linkIndex < links.Length; linkIndex++)
            {
                var link = links[linkIndex];

                MethodInfo? method = null;

                if (link is ClosureLambdaLink closureLink)
                {
                    var nestedTypes = link.ClassType.GetNestedTypes(BindingFlags.NonPublic);

                    var expectedTypeName = "<>DisplayClass" + closureLink.MemberIndex;

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
                            link.FullLambdaName,
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
                    method =
                        link.ClassType.GetMethod(
                            link.FullLambdaName,
                            BindingFlags.NonPublic | BindingFlags.Instance
                        ) ?? throw new InvalidOperationException();
                }

                lambdaData.Add(method, link.Data);
            }

            _lambdaData = lambdaData;

            linksField.SetValue(null, null);
        }

        internal static LambdaData GetLambdaData(MethodInfo method)
        {
            return _lambdaData[method];
        }
    }
}
