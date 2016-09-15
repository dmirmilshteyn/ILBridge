using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using System.IO;
using ILBridge.Decompiler;
using Mono.Cecil;

namespace ILBridge
{
    class Program
    {
        static void Main(string[] args) {
            var app = new CommandLineApplication();

            app.Command("transpile", c =>
            {
                var inputAssemblyOption = c.Argument("input", "Input assembly to be transpiled", false);
                var outputDirectoryOption = c.Option("-o|--output", "Output directory for transpiled javascript", CommandOptionType.SingleValue);

                c.OnExecute(() =>
                {
                    if (!File.Exists(inputAssemblyOption.Value)) {
                        Console.Error.WriteLine($"Input assembly \"{inputAssemblyOption.Value}\" has not been found.");
                        return 1;
                    }

                    string outputDirectory = outputDirectoryOption.HasValue() ? outputDirectoryOption.Value() : Path.Combine(Directory.GetCurrentDirectory(), "ILBridge-Output");
                    if (!Directory.Exists(outputDirectory)) {
                        Directory.CreateDirectory(outputDirectory);
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
                    var inputAssemblyDirectory = Path.GetDirectoryName(inputAssemblyOption.Value);
                    var inputAssemblyWorkingDirectory = Path.Combine(workingDirectory, inputAssemblyName);

                    var inputAssembly = AssemblyStatus.BuildAssemblyStatus(workingDirectory, inputAssemblyOption.Value);

                    RecursiveDecompile(decompiler, inputAssembly);

                    // Prepare transpiler
                    var transpiler = new Transpiler.Bridge.BridgeTranspiler();

                    var toolsDirectory = Path.Combine(cacheDirectory, "Tools", transpiler.Name);
                    if (!Directory.Exists(toolsDirectory)) {
                        Directory.CreateDirectory(toolsDirectory);
                    }

                    transpiler.ConfigureTools(toolsDirectory);
                    transpiler.GenerateConfiguration(inputAssembly, outputDirectory);
                    transpiler.Transpile();

                    // Cleanup
                    //Directory.Delete(workingDirectory, true);

                    return 0;
                });
            });

            app.Execute(args);
        }

        private static void RecursiveDecompile(IDecompiler decompiler, AssemblyStatus inputAssembly) {
            Directory.CreateDirectory(inputAssembly.WorkingDirectory);
            decompiler.Decompile(inputAssembly.AssemblyPath, inputAssembly.WorkingDirectory);

            foreach (var childAssembly in inputAssembly.References) {
                RecursiveDecompile(decompiler, childAssembly);
            }
        }
    }
}
