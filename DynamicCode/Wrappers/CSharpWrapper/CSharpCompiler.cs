using Contracts;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Threading.Tasks;

namespace CSharpWrapper
{
    public class CSharpCompiler : Compiler
    {
        private CSharpCodeProvider _compiler;

        public CSharpCompiler(CompilerOptions options)
            : base(options)
        {
            _compiler = new CSharpCodeProvider();
        }

        public override async Task<CompilerResult> CompileAsync(string code)
        {
            var assemblyPath = Path.Combine(Options.TemporaryCompilationLocation, $"{Options.AssemblyName}.data");
            CompilerResult result;
            if (File.Exists(assemblyPath))
            {
                File.Delete(assemblyPath);
            }

            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = Options.OutputType != OutputType.DynamicLinkLibrary,
                GenerateInMemory = false,
                TreatWarningsAsErrors = true,
                IncludeDebugInformation = false,
                OutputAssembly = assemblyPath                
            };

            if (Options.ReferencesAssemblyLocations != null)
            {
                compilerParameters.ReferencedAssemblies.AddRange(Options.ReferencesAssemblyLocations);
            }

            try
            {
                var compilationResult = _compiler.CompileAssemblyFromSource(compilerParameters, code);
                result = new CompilerResult();
                if (compilationResult.Errors.Count > 0)
                {
                    foreach (var compilationError in compilationResult.Errors)
                    {
                        result.Errors.Add(compilationError.ToString());
                    }
                }
                else
                {
                    using (var fileStream = File.OpenRead(assemblyPath))
                    {
                        using (var ms = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(ms);
                            result.Assembly = ms.ToArray();
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CompilerException(ex.Message, ex);
            }

            if (File.Exists(assemblyPath))
            {
                File.Delete(assemblyPath);
            }

            return result;
        }

        protected override void DisposeCore(bool disposing)
        {
            if (disposing)
            {
                _compiler?.Dispose();
                _compiler = null;
            }
        }
    }
}
