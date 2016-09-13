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

                    var decompiler = new JustDecompileDecompiler();

                    var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ILBridge-Temp");
                    if (Directory.Exists(workingDirectory)) {
                        Directory.Delete(workingDirectory, true);
                    }
                    Directory.CreateDirectory(workingDirectory);

                    var inputAssemblyName = Path.GetFileNameWithoutExtension(inputAssemblyOption.Value);
                    var inputAssemblyWorkingDirectory = Path.Combine(workingDirectory, inputAssemblyName);
                    Directory.CreateDirectory(inputAssemblyWorkingDirectory);

                    decompiler.Decompile(inputAssemblyOption.Value, inputAssemblyWorkingDirectory);

                    return 0;
                });
            });

            app.Execute(args);
        }
    }
}
