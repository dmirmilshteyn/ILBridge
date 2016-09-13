using JustDecompileCmdShell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILBridge.Decompiler
{
    class JustDecompileDecompiler : IDecompiler
    {
        string inputAssembly;
        string resultsDirectory;

        public void Decompile(string inputAssembly, string resultsDirectory) {
            this.inputAssembly = inputAssembly;
            this.resultsDirectory = resultsDirectory;

            var generatorProjectInfo = BuildGeneratorProjectInfo();
            CmdShell shell = new CmdShell();
            shell.Run(generatorProjectInfo);

            PostProcess(resultsDirectory);
        }

        private GeneratorProjectInfo BuildGeneratorProjectInfo() {
            var projectInfo = new GeneratorProjectInfo(inputAssembly, resultsDirectory);
            projectInfo.AddDocumentation = false;

            return projectInfo;
        }

        private void PostProcess(string resultsDirectory) {
            // Delete AssemblyInfo to clean up any extra attributes that have been added
            var assemblyInfoPath = Path.Combine(resultsDirectory, "Properties", "AssemblyInfo.cs");

            if (File.Exists(assemblyInfoPath)) {
                File.Delete(assemblyInfoPath);
            }
        }
    }
}
