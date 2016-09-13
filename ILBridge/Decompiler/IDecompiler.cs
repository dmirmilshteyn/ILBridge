using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILBridge.Decompiler
{
    interface IDecompiler
    {
        void Decompile(string inputAssembly, string resultsDirectory);
    }
}
