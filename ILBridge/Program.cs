using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using System.IO;
using ILBridge.Decompiler;

namespace ILBridge
{
    class Program
    {
        static void Main(string[] args) {
            var app = new CommandLineApplication();

            app.Command("transpile", c =>
            {
                var inputAssemblyOption = c.Argument("input", "Input assembly to be transpiled", false);

                c.OnExecute(() =>
                {
                    if (!File.Exists(inputAssemblyOption.Value)) {
                        Console.Error.WriteLine($"Input assembly \"{inputAssemblyOption.Value}\" has not been found.");
                        return 1;
                    }

                    // Prepare decompiler
                    var decompiler = new JustDecompileDecompiler();

                    var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ILBridge-Temp");
                    if (Directory.Exists(workingDirectory)) {
                        Directory.Delete(workingDirectory, true);
                    }
                    Directory.CreateDirectory(workingDirectory);

                    var cacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ILBridge-Cache");
                    if (!Directory.Exists(cacheDirectory)) {
                        Directory.CreateDirectory(cacheDirectory);
                    }

                    var inputAssemblyName = Path.GetFileNameWithoutExtension(inputAssemblyOption.Value);
                    var inputAssemblyWorkingDirectory = Path.Combine(workingDirectory, "Decompiled", inputAssemblyName);
                    Directory.CreateDirectory(inputAssemblyWorkingDirectory);

                    decompiler.Decompile(inputAssemblyOption.Value, inputAssemblyWorkingDirectory);

                    // Prepare transpiler
                    var transpiler = new Transpiler.Bridge.BridgeTranspiler();

                    var toolsDirectory = Path.Combine(cacheDirectory, "Tools", transpiler.Name);
                    if (!Directory.Exists(toolsDirectory)) {
                        Directory.CreateDirectory(toolsDirectory);
                    }

                    transpiler.ConfigureTools(toolsDirectory);

                    return 0;
                });
            });

            app.Execute(args);
        }
    }
}
