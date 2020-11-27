using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Venflow.Tools.CLI.Commands
{
    internal abstract class CommandHandlerBase : ICommandHandler
    {
        protected InvocationContext Context { get; private set; }

        async Task<int> ICommandHandler.InvokeAsync(InvocationContext context)
        {
            Context = context;

            await HandleAsync();

            return 0;
        }

        protected abstract Task HandleAsync();

        [return: MaybeNull]
        protected T GetArgumentValue<T>(Argument<T> argument)
            => Context.ParseResult.ValueForArgument(argument);
    }
}
