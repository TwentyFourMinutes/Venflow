using System;
using System.Runtime.InteropServices;
using System.Threading;
using DulcisX.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Venflow.VSIX.Commands;
using Task = System.Threading.Tasks.Task;

namespace Venflow.VSIX
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.guidVenflowVSIXPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideUIContextRule(PackageGuids.uiContextCommandSupportedFilesString,
    name: "Supported Files",
    expression: "SolutionExists & (SingleProject | MultipleProjects) & DotCSharpHtml",
    termNames: new[] { "SolutionExists", "SingleProject", "MultipleProjects", "DotCSharpHtml" },
    termValues: new[] { VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, VSConstants.UICONTEXT.SolutionHasSingleProject_string, VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, "HierSingleSelectionName:.cs$" })]
    public sealed class Venflow : PackageX
    {
        public Venflow()
        {
            base.OnInitializeAsync += InitAsync;
        }

        public async Task InitAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            progress.Report(new ServiceProgressData("Initializing", string.Empty, 1, 1));

            if (Solution.IsTempSolution())
                return;

            await CommandHandler.InitializeAsync(this);
        }
    }
}
