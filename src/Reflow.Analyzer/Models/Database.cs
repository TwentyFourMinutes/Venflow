using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Sections.LambdaSorter;

namespace Reflow.Analyzer.Models
{
    internal class Database
    {
        internal ITypeSymbol Symbol { get; }
        internal Dictionary<ITypeSymbol, Entity> Entities { get; }

        internal List<Query> Queries { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "MicrosoftCodeAnalysisCorrectness",
            "RS1024:Compare symbols correctly",
            Justification = "<Pending>"
        )]
        internal Database(ITypeSymbol symbol)
        {
            Symbol = symbol;
            Entities = new(SymbolEqualityComparer.Default);
            Queries = new();
        }
    }
}
