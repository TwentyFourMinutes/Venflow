using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Venflow.Tools.CLI
{
    internal static class MSBuild
    {
        internal static async Task<string> GetAssemblyPathFromProjectAsync(string fullName)
        {
            var projectName = Path.GetFileNameWithoutExtension(fullName);
            var projectDirectoryPath = Path.GetDirectoryName(fullName);

            await BuildProjectAsync(fullName);

            var targetFilePath = Path.Combine(projectDirectoryPath, "obj", projectName + ".csproj.Venflow.targets");

            using (var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("Venflow.Tools.CLI.Resources.Venflow.targets"))
            using (var fs = new FileStream(targetFilePath,
                   FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                   4096, FileOptions.RandomAccess))
            {
                await rs.CopyToAsync(fs);
            }

            var metadataFilePath = Path.GetTempFileName();

            string rawMetadata;

            try
            {
                var properties = $"msbuild {projectName}.csproj /target:GetVenflowProjectMetadata /verbosity:quiet /nologo /property:MetadataFile=" + metadataFilePath;

                if (await ExecuteProcessAsync(projectDirectoryPath, properties) != 0)
                {
                    throw new CommandException($"Targets build failed, ensure that your '{projectName}.csproj' file is valid.");
                }

                rawMetadata = await File.ReadAllTextAsync(metadataFilePath);
            }
            finally
            {
                if (File.Exists(metadataFilePath))
                    File.Delete(metadataFilePath);
            }

            var metadata = rawMetadata.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(x => x.Split(": ", 2)).ToDictionary(x => x[0], x => x[1]);

            var assemblyFilePath = Path.Combine(metadata["ProjectDir"], metadata["OutputPath"], metadata["TargetFileName"]);

            if (!File.Exists(assemblyFilePath))
                throw new CommandException("The assembly file couldn't automatically determined, please specify it manually.");

            return assemblyFilePath;
        }

        internal static async Task BuildProjectAsync(string fullName)
        {
            if (await ExecuteProcessAsync(Path.GetDirectoryName(fullName), "build " + Path.GetFileName(fullName)) != 0)
            {
                throw new CommandException($"Project build failed, ensure that your '{Path.GetFileName(fullName)}' file is valid and your code-base builds.");
            }
        }

        private static Task<int> ExecuteProcessAsync(string workingDirectory, string args)
        {
            var processInfo = new ProcessStartInfo
            {
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "dotnet",
                WorkingDirectory = workingDirectory
            };

            var process = new Process();

            var tcs = new TaskCompletionSource<int>();

            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) =>
            {
                tcs.SetResult(process.ExitCode);

                process.Dispose();
            };

            process.Start();
            process.EnableRaisingEvents = true;
            return tcs.Task;
        }
    }
}
