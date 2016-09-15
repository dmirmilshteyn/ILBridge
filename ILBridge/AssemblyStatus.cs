using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILBridge
{
    class AssemblyStatus {
        List<AssemblyStatus> references;

        public string WorkingDirectory { get; }
        public string AssemblyPath { get; }
        public string AssemblyName { get; }
        public string AssemblyDirectory { get; }
        public string PipelineWorkingDirectory { get; }

        public string CompiledAssemblyPath { get; set; }

        public IReadOnlyList<AssemblyStatus> References {
            get { return references; }
        }

        private AssemblyStatus(string pipelineWorkingDirectory, string workingDirectory, string assemblyPath) {
            this.PipelineWorkingDirectory = pipelineWorkingDirectory;
            this.WorkingDirectory = workingDirectory;
            this.AssemblyPath = assemblyPath;

            this.AssemblyName = Path.GetFileNameWithoutExtension(this.AssemblyPath);
            this.AssemblyDirectory = Path.GetDirectoryName(this.AssemblyPath);

            this.references = new List<AssemblyStatus>();
        }

        private void CacheReferences() {
            // Load all assembly references
            using (var module = ModuleDefinition.ReadModule(AssemblyPath)) {
                foreach (var reference in AssemblyReferenceLoader.LoadAssemblyReferences(module)) {
                    var extensions = new string[]
                    {
                        ".exe",
                        ".dll"
                    };
                    string assemblyDirectory = Path.GetDirectoryName(AssemblyPath);
                    foreach (var extension in extensions) {
                        var referencePath = Path.Combine(assemblyDirectory, reference + extension);

                        if (File.Exists(referencePath)) {
                            references.Add(AssemblyStatus.BuildAssemblyStatus(PipelineWorkingDirectory, referencePath));
                            break;
                        }
                    }
                }
            }
        }

        public static AssemblyStatus BuildAssemblyStatus(string pipelineWorkingDirectory, string assemblyPath) {
            var workingDirectory = Path.Combine(pipelineWorkingDirectory, Path.GetFileNameWithoutExtension(assemblyPath));

            var assemblyStatus  = new AssemblyStatus(pipelineWorkingDirectory, workingDirectory, assemblyPath);
            assemblyStatus.CacheReferences();

            return assemblyStatus;
        }
    }
}
