using Contracts;
using CSharpWrapper;
using RoslynCompilerWrapper;
using System;
using System.Linq;
using System.Reflection;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var refs = new[]
                {
                    typeof(object).Assembly.Location,
                    typeof(Console).Assembly.Location,
                    typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location
                };

            var compilerOptions = new CompilerOptions
            {
                OutputType = OutputType.DynamicLinkLibrary,
                AssemblyName = "DynamicCodeGenerationTest",
                TemporaryCompilationLocation = "C:\\_temp",
                ReferencesAssemblyLocations = refs.Distinct().ToArray()
            };

            var code = @"
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
                Console.WriteLine($""\t {arg}"");
            }
        }
    }
}
";

            var compiler = new CSharpCompiler(compilerOptions);
            var compiler = new RoslynCompiler(compilerOptions);

            var compilerResult = compiler.CompileAsync(code)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            if (compilerResult.IsSuccessful())
            {
                var assembly = Assembly.Load(compilerResult.Assembly);
                var classDefinition = assembly.GetExportedTypes()
                    .First(w => w.IsClass && w.Name == "DynamicTestClass");

                var instnace = Activator.CreateInstance(classDefinition);
                var argsArray = new object[]
                {
                    new string[]
                    {
                        "arg1",
                        "arg2",
                        "arg3"
                    }
                };
                classDefinition.GetMethod("Run").Invoke(instnace, argsArray);
            }
            else
            {
                foreach (var error in compilerResult.Errors)
                {
                    Console.WriteLine(error);
                }
            }

            Console.ReadLine();
        }
    }
}
