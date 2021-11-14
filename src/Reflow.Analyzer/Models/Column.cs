using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Models
{
    internal class Column
    {
        internal IPropertySymbol Symbol { get; }

        internal Column(IPropertySymbol symbol)
        {
            Symbol = symbol;
        }
    }
}
