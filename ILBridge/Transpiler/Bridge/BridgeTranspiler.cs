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
        private static readonly string ToolsUrl = @"https://raw.githubusercontent.com/bridgedotnet/Archives/master/15.0.0/Bridge.NET.VSCode.15.0.0.zip";

        public string Name { get; } = "Bridge";

        public string BridgeBuilderDirectory { get; private set; }
        public AssemblyStatus CoreAssembly { get; private set; }
        public string OutputDirectory { get; private set; }

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

        public void GenerateConfiguration(AssemblyStatus coreAssembly, string outputDirectory) {
            this.OutputDirectory = outputDirectory;
            this.CoreAssembly = coreAssembly;

            var projectOutputDirectory = Path.Combine(outputDirectory, coreAssembly.AssemblyName);
            var buildDirectory = Path.Combine(coreAssembly.WorkingDirectory, ".build");

            Directory.CreateDirectory(buildDirectory);
            foreach (var file in Directory.EnumerateFiles(BridgeBuilderDirectory)) {
                File.Copy(file, Path.Combine(buildDirectory, Path.GetFileName(file)));
            }

            using (var template = File.CreateText(Path.Combine(coreAssembly.WorkingDirectory, "bridge.json"))) {
                template.WriteLine("{");
                template.WriteLine($"	\"output\": \"{projectOutputDirectory.Replace('\\', '/')}\",");
                //template.WriteLine("    \"logging\": { \"level\": \"info\" }");
                template.WriteLine("}");
            }

            if (Directory.Exists(projectOutputDirectory)) {
                Directory.Delete(projectOutputDirectory, true);
            }
            Directory.CreateDirectory(projectOutputDirectory);
        }

        public void Transpile() {
            // Transpile references assemblies
            foreach (var referenceAssembly in CoreAssembly.References) {
                var transpiler = new BridgeTranspiler();
                transpiler.BridgeBuilderDirectory = BridgeBuilderDirectory;
                transpiler.GenerateConfiguration(referenceAssembly, OutputDirectory);
                transpiler.Transpile();

                File.Copy(referenceAssembly.CompiledAssemblyPath, Path.Combine(CoreAssembly.WorkingDirectory, ".build", referenceAssembly.AssemblyName + ".dll"));
            }

            Console.WriteLine(GenerateReferenceString(CoreAssembly));

            ExecuteProcess(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe", $"/nostdlib /target:library /warn:0 /reference:{GenerateReferenceString(CoreAssembly)} /out:.build\\compiled.dll /recurse:*.cs", CoreAssembly.WorkingDirectory);
            ExecuteProcess(Path.Combine(CoreAssembly.WorkingDirectory, ".build", "Bridge.Builder.exe"), $"\"{Path.Combine(".build", "compiled.dll")}\"", CoreAssembly.WorkingDirectory);

            CoreAssembly.CompiledAssemblyPath = Path.Combine(CoreAssembly.WorkingDirectory, ".build", "compiled.dll");
        }

        private string GenerateReferenceString(AssemblyStatus assembly) {
            var referencesList = new List<string>();

            // Add bridge dependencies
            referencesList.Add($"\"{Path.Combine(BridgeBuilderDirectory, "Bridge.dll")}\"");
            referencesList.Add($"\"{Path.Combine(BridgeBuilderDirectory, "Bridge.Html5.dll")}\"");

            foreach (var reference in assembly.References) {
                if (string.IsNullOrEmpty(reference.CompiledAssemblyPath)) {
                    referencesList.Add($"\"{reference.AssemblyPath}\"");
                } else {
                    referencesList.Add($"\"{reference.CompiledAssemblyPath}\"");
                }
            }

            return string.Join(";", referencesList);
        }

        private void ExecuteProcess(string processPath, string arguments, string workingDirectory) {
            var startInfo = new ProcessStartInfo(processPath);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.WorkingDirectory = CoreAssembly.WorkingDirectory;
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
