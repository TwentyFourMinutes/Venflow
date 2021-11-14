using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Sections.LambdaSorter;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class QueriesEmitter
    {
        //internal static SourceText Emit(Database database, List<Query> queries)
        //{
        //    var materializers = new MethodDeclarationSyntax[queries.Count];

        //    for (var queryIndex = 0; queryIndex < queries.Count; queryIndex++)
        //    {
        //        var query = queries[queryIndex];

        //        materializers[queryIndex] =
        //           Method(query);
        //    }

        //    return File("Reflow.Queries")
        //        .WithMembers(
        //            Class($"__{database.Symbol.GetFullName().Replace('.', '_')}", CSharpModifiers.Public | CSharpModifiers.Static)
        //                .WithMembers(

        //                )
        //        )
        //        .GetText();

        //    static TypeSyntax DictionaryType() =>
        //        GenericType(
        //            typeof(Dictionary<,>),
        //            Type(typeof(Type)),
        //            Array(Type(typeof(string)))
        //        );
        //}
    }
}
