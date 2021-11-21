using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer.Models
{
    internal interface ICommandOperation
    {
        FluentCallDefinition FluentCall { get; }
    }
}
