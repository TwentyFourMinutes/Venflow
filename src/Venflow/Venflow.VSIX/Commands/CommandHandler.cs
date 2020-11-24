using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DulcisX.Core;
using DulcisX.Nodes;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Venflow.VSIX.Commands
{
    internal sealed class CommandHandler
    {
        public static CommandHandler Instance { get; private set; }

        private readonly PackageX _package;

        private CommandHandler(PackageX package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            AddCommand(commandService, PackageIds.CreateMigrationCommandId, CreateMigrationAsync);
            AddCommand(commandService, PackageIds.UpdateDatabaseCommandId, CreateMigrationAsync);
        }

        private void AddCommand(OleMenuCommandService commandService, int commandId, Action callback)
        {
            var menuCommandID = new CommandID(PackageGuids.guidVenflowVSIXPackageCmdSet, commandId);
            var menuItem = new MenuCommand((s, e) => callback(), menuCommandID);

            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(PackageX package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CommandHandler(package, commandService);
        }

        private async void CreateMigrationAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var selectedNode = _package.Solution.SelectedNodes.FirstOrDefault() as DocumentNode;

            if (selectedNode is null)
                return;

            var parentProject = selectedNode.GetParentProject();

            if (parentProject is null)
                return;

            var dte = _package.GetService<DTE, DTE2>();

            Action<bool, bool, bool> buildCallback = default;
            Action buildCancelCallback = default;

            var tcs = new TaskCompletionSource<bool>();

            buildCallback = (x, y, z) =>
            {
                _package.Solution.SolutionBuildEvents.OnSolutionBuilt -= buildCallback;
                _package.Solution.SolutionBuildEvents.OnSolutionBuildCancel -= buildCancelCallback;

                tcs.SetResult(true);
            };

            buildCancelCallback = () =>
            {
                _package.Solution.SolutionBuildEvents.OnSolutionBuilt -= buildCallback;
                _package.Solution.SolutionBuildEvents.OnSolutionBuildCancel -= buildCancelCallback;

                tcs.SetResult(false);
            };

            _package.Solution.SolutionBuildEvents.OnSolutionBuilt += buildCallback;
            _package.Solution.SolutionBuildEvents.OnSolutionBuildCancel += buildCancelCallback;

            var projectFullName = parentProject.GetFullName();

            dte.Solution.SolutionBuild.BuildProject(dte.Solution.SolutionBuild.ActiveConfiguration.Name, projectFullName);

            if (!await tcs.Task)
                return;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (dte.Solution.SolutionBuild.LastBuildInfo != 0)
                return;

            var dteProject = dte.Solution.Projects.OfType<Project>().FirstOrDefault(x => x.FullName == projectFullName);

            var dllOutputPath = (string)dteProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value;

            var outputDllPath = Path.Combine(Path.GetDirectoryName(parentProject.GetFullName()), dllOutputPath, parentProject.GetDisplayName() + ".dll");

            if (!File.Exists(outputDllPath))
                return;


        }
    }
}
