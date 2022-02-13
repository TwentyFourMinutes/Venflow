using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer.Models
{
    internal interface IOperation
    {
        FluentCallDefinition FluentCall { get; }
    }
}
