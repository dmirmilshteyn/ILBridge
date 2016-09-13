using JustDecompileCmdShell;
using System;
using System.Collections.Generic;
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
        }

        private GeneratorProjectInfo BuildGeneratorProjectInfo() {
            var projectInfo = new GeneratorProjectInfo(inputAssembly, resultsDirectory);
            projectInfo.AddDocumentation = false;

            return projectInfo;
        }
    }
}
