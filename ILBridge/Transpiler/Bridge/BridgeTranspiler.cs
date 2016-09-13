using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ILBridge.Transpiler.Bridge
{
    class BridgeTranspiler : ITranspiler
    {
        private static readonly string ToolsUrl = @"https://github.com/bridgedotnet/Archives/blob/master/1.13.0/Bridge.NET.VSCode.1.13.0.zip?raw=true";

        public string Name { get; } = "Bridge";

        public string BridgeBuilderDirectory { get; private set; }
        public string WorkingDirectory { get; private set; }
        public string ProjectName { get; private set; }

        public void ConfigureTools(string toolsDirectory) {
            var bridgeToolsDirectory = Path.Combine(toolsDirectory, "Tools");

            if (!Directory.Exists(bridgeToolsDirectory)) {
                var toolsArchivePath = Path.Combine(toolsDirectory, "tools.zip");

                var webClient = new WebClient();
                webClient.DownloadFile(ToolsUrl, toolsArchivePath);

                using (var toolsStream = new FileStream(toolsArchivePath, FileMode.Open, FileAccess.Read)) {
                    using (var toolsArchive = new ZipArchive(toolsStream, ZipArchiveMode.Read)) {
                        toolsArchive.ExtractToDirectory(bridgeToolsDirectory);
                    }
                }

                File.Delete(toolsArchivePath);
            }

            string builderDirectory;
            if (!TryFindBuilderDirectory(bridgeToolsDirectory, out builderDirectory)) {
                throw new DirectoryNotFoundException($"Bridge builder directory not found. Searched in: {bridgeToolsDirectory}");
            } else {
                Console.WriteLine($"Using bridge builder directory: {builderDirectory}");

                BridgeBuilderDirectory = builderDirectory;
            }
        }

        private bool TryFindBuilderDirectory(string baseDirectory, out string builderDirectory) {
            var bridgeBuilderPath = Directory.EnumerateFiles(baseDirectory, "Bridge.Builder.exe", SearchOption.AllDirectories).FirstOrDefault();

            if (string.IsNullOrEmpty(bridgeBuilderPath)) {
                builderDirectory = null;
                return false;
            } else {
                builderDirectory = Path.GetDirectoryName(bridgeBuilderPath);
                return true;
            }
        }

        public void GenerateConfiguration(string projectName, string workingDirectory, string outputDirectory) {
            this.WorkingDirectory = workingDirectory;
            this.ProjectName = projectName;

            var projectOutputDirectory = Path.Combine(outputDirectory, projectName);
            var buildDirectory = Path.Combine(workingDirectory, ".build");

            Directory.CreateDirectory(buildDirectory);
            foreach (var file in Directory.EnumerateFiles(BridgeBuilderDirectory)) {
                File.Copy(file, Path.Combine(buildDirectory, Path.GetFileName(file)));
            }

            using (var template = File.CreateText(Path.Combine(workingDirectory, "bridge.json"))) {
                template.WriteLine("{");
                template.WriteLine($"	\"output\": \"{projectOutputDirectory.Replace('\\', '/')}\"");
                template.WriteLine("}");
            }

            if (Directory.Exists(projectOutputDirectory)) {
                Directory.Delete(projectOutputDirectory, true);
            }
            Directory.CreateDirectory(projectOutputDirectory);
        }

        public void Transpile() {
            ExecuteProcess(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe", $"/nostdlib /noconfig /warn:0 /reference:\"{Path.Combine(BridgeBuilderDirectory, "Bridge.dll")}\";\"{Path.Combine(BridgeBuilderDirectory, "Bridge.Html5.dll")}\" /out:.build\\compiled.dll /recurse:*.cs", WorkingDirectory);
            ExecuteProcess(Path.Combine(WorkingDirectory, ".build", "Bridge.Builder.exe"), $"-lib \"{Path.Combine(".build", "compiled.dll")}\"", WorkingDirectory);
        }

        private void ExecuteProcess(string processPath, string arguments, string workingDirectory) {
            var startInfo = new ProcessStartInfo(processPath);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.WorkingDirectory = WorkingDirectory;
            startInfo.UseShellExecute = false;
            startInfo.Arguments = arguments;

            Console.WriteLine(arguments);
            var process = Process.Start(startInfo);
            process.WaitForExit();

            Console.WriteLine(process.StandardOutput.ReadToEnd());
            Console.WriteLine(process.StandardError.ReadToEnd());
        }
    }
}
