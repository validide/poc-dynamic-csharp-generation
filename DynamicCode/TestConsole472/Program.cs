using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Contracts;
using CSharpWrapper;
using RoslynCompilerWrapper;

namespace TestConsole472
{
    internal class Program
    {
        private static async Task Main()
        {
            var assemblyDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? "C:\\";
            var tempBuildLocation = Path.Combine(assemblyDirectory, "_build");
            Console.WriteLine("Using \"{0}\" as build location", tempBuildLocation);
            if (Directory.Exists(tempBuildLocation))
            {
                Directory.Delete(tempBuildLocation, true);
            }

            Directory.CreateDirectory(tempBuildLocation);

            var compilers = new Compiler[]
            {
                new CSharpCompiler(new CompilerOptions
                {
                    OutputType = OutputType.DynamicLinkLibrary,
                    AssemblyName = AssemblyName,
                    TemporaryCompilationLocation = tempBuildLocation,
                    ReferencesAssemblyLocations = null
                }),
                new RoslynCompiler(new CompilerOptions
                {
                    OutputType = OutputType.DynamicLinkLibrary,
                    AssemblyName = AssemblyName,
                    ReferencesAssemblyLocations = new[]
                        {
                            typeof(object).Assembly.Location
                            //typeof(Console).Assembly.Location,
                            //typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location
                        }
                        .Distinct()
                        .ToArray()
                })
            };


            foreach (var code in new[] { InvalidCode, ValidCode })
            {
                Console.WriteLine("\n\nCompiling the following code:");
                Console.WriteLine(code);

                foreach (var compiler in compilers)
                {
                    Console.WriteLine("\n\nCompiler: {0}", compiler.GetType().Name);
                    var compilerResult = await compiler.CompileAsync(code);

                    if (compilerResult.IsSuccessful())
                    {

                        var assembly = Assembly.Load(compilerResult.Assembly);
                        var classDefinition = assembly.GetExportedTypes()
                            .First(w => w.IsClass && w.Name == "DynamicTestClass");

                        var instance = Activator.CreateInstance(classDefinition);
                        var argsArray = new object[]
                        {
                            new[]
                            {
                                "arg1",
                                "arg2",
                                "arg3"
                            }
                        };
                        // ReSharper disable once PossibleNullReferenceException
                        classDefinition.GetMethod("Run").Invoke(instance, argsArray);
                    }
                    else
                    {
                        foreach (var error in compilerResult.Errors)
                        {
                            Console.WriteLine(error);
                        }
                    }
                }
            }

            Console.ReadLine();
        }

        private const string AssemblyName = "DynamicCodeGenerationTest";

        private const string ValidCode = @"
using System;

namespace DynamicTestNamespace
{
    public class DynamicTestClass
    {
        public void Run(string[] args)
        {
            Console.WriteLine(""Hello World!"");
            foreach (var arg in args)
            {
                Console.WriteLine(""\t {0}"", arg); //string interpolation is not supported
            }
        }
    }
}
";

        private const string InvalidCode = @"
using System;

namespace DynamicTestNamespace
{
    public class DynamicTestClass
    {
        public Run(string[] args)
        {
            Console.WriteLine(""Hello World!"");
            foreach (var arg in args)
            {
                Console.WriteLine($""\t {arg}"");
            }
        }
    }
}
";
    }
}
