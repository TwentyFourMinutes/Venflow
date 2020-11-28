using System;
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

            try
            {
                await HandleAsync();
            }
            catch (CommandException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex.Message);

                Console.ForegroundColor = ConsoleColor.Gray;
            }
            return 0;
        }

        protected abstract Task HandleAsync();

        [return: MaybeNull]
        protected T GetArgumentValue<T>(Argument<T> argument)
            => Context.ParseResult.ValueForArgument(argument);

        [return: MaybeNull]
        protected T GetOptionValue<T>(Option<T> option)
            => Context.ParseResult.ValueForOption(option);
    }
}
