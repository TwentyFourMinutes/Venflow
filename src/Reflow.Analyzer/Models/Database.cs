using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Sections.LambdaSorter;

namespace Reflow.Analyzer.Models
{
    internal class Database
    {
        internal ITypeSymbol Symbol { get; }
        internal List<Entity> Entities { get; }

        internal List<Query> Queries { get; }

        internal Database(ITypeSymbol symbol)
        {
            Symbol = symbol;
            Entities = new();
            Queries = new();
        }
    }
}
