using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using System.IO;

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

                    return 0;
                });
            });

            app.Execute(args);
        }
    }
}
