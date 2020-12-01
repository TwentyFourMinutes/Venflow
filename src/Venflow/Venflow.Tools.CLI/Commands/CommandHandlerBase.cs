using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
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
                await HandleAsync(await GetProjectMetadataAsync());
            }
            catch (CommandException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex.Message);

                Console.ForegroundColor = ConsoleColor.Gray;
            }
            return 0;
        }

        private async Task<CommandContext> GetProjectMetadataAsync()
        {
            var assemblyPath = GetOptionValue(AppRootCommand.AssemblyPathOption);

            var projectName = GetOptionValue(AppRootCommand.ProjectNameOption);

            var projectPath = default(string);

            if (projectName is { })
            {
                if (projectName.EndsWith(".csproj"))
                {
                    if (File.Exists(projectName))
                    {
                        projectPath = Path.Combine(Directory.GetCurrentDirectory(), projectName);
                    }
                    else
                    {
                        throw new CommandException($"The current directory doesn't contain the file named '{projectName}'.");
                    }
                }

                if (projectPath is null &&
                    Directory.Exists(projectName))
                {
                    var multipleMatches = false;

                    foreach (var fileName in Directory.EnumerateFiles(projectName, "*.csproj", SearchOption.TopDirectoryOnly))
                    {
                        if (multipleMatches)
                            throw new CommandException($"The directory '{Path.Combine(Directory.GetCurrentDirectory(), fileName)}' contains multiple '.csproj' files. Please be more specific by either providing the full path or the full relative path.");

                        projectPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                        multipleMatches = true;
                    }
                }

                if (projectPath is null)
                {
                    throw new CommandException($"The project file couldn't be found. Searched location: '{Path.Combine(Directory.GetCurrentDirectory(), projectName)}'.");
                }
            }
            else
            {
                var multipleMatches = false;

                foreach (var fileName in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.TopDirectoryOnly))
                {
                    if (multipleMatches)
                        throw new CommandException($"The directory '{Path.Combine(Directory.GetCurrentDirectory(), fileName)}' contains multiple '.csproj' files. Please be more specific by either providing the full path or the full relative path.");

                    projectPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                    multipleMatches = true;
                }

                if (projectPath is null)
                {
                    throw new CommandException($"The project file couldn't be found. Searched location: '{Path.Combine(Directory.GetCurrentDirectory())}'.");
                }
            }

            Console.WriteLine("Building ...");

            if (assemblyPath is null)
            {
                assemblyPath = await MSBuild.GetAssemblyPathFromProjectAsync(projectPath);
            }
            else
            {
                await MSBuild.BuildProjectAsync(projectPath);
            }

            Console.WriteLine("Build successful.");

            var contextName = GetOptionValue(AppRootCommand.ContextNameOption);

            var migrationAssembly = Assembly.LoadFrom(assemblyPath);

            var designAssemblyPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Venflow.Design.dll");

            if (!File.Exists(designAssemblyPath))
                throw new CommandException($"The project `{migrationAssembly.FullName}` doesn't contain the 'Venflow.Design' NuGet package.");

            var designAssembly = Assembly.LoadFrom(designAssemblyPath);

            if (contextName is { })
            {
                return new CommandContext(projectPath, new MigrationHandler(designAssembly, migrationAssembly, contextName));
            }
            else
            {
                return new CommandContext(projectPath, new MigrationHandler(designAssembly, migrationAssembly));
            }
        }

        protected abstract Task HandleAsync(CommandContext commandContext);

        [return: MaybeNull]
        protected T GetArgumentValue<T>(Argument<T> argument)
            => Context.ParseResult.ValueForArgument(argument);

        [return: MaybeNull]
        protected T GetOptionValue<T>(Option<T> option)
            => Context.ParseResult.ValueForOption(option);
    }
}
