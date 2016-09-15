using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILBridge
{
    class AssemblyReferenceLoader
    {
        public static IEnumerable<string> LoadAssemblyReferences(ModuleDefinition assembly) {
            foreach (var reference in assembly.AssemblyReferences) {
                switch (reference.Name) {
                    case "mscorlib":
                        break;
                    default:
                        yield return reference.Name;
                        break;
                }
            }
        }
    }
}
