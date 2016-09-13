using System;
using System.Collections.Generic;
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

        public void GenerateConfiguration(string projectName, string workingDirectory) {

        }
    }
}
