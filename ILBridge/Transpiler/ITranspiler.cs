using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILBridge.Transpiler
{
    interface ITranspiler
    {
        string Name { get; }
        AssemblyConfiguration CoreAssembly { get; }

        void ConfigureTools(string toolsDirectory);
        void GenerateConfiguration(AssemblyConfiguration coreAssembly, string outputDirectory);
        void Transpile();
    }
}
