using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynCompilerWrapper
{
    public class RoslynCompiler : Compiler
    {
        public RoslynCompiler(CompilerOptions options)
            : base(options)
        {
        }

        public override Task<CompilerResult> CompileAsync(string code)
        {
            try
            {
                var references = Options
                    .ReferencesAssemblyLocations
                    ?.Select(l => MetadataReference.CreateFromFile(l))
                    ?? (IEnumerable<MetadataReference>)new MetadataReference[0];
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var options = new CSharpCompilationOptions(
                    Options.OutputType == OutputType.DynamicLinkLibrary
                        ? OutputKind.DynamicallyLinkedLibrary
                        : OutputKind.ConsoleApplication
                );
                var compilation = CSharpCompilation.Create(
                    Options.AssemblyName,
                    new SyntaxTree[] { syntaxTree },
                    references,
                    options
                );

                using (var ms = new MemoryStream())
                {
                    var compilationResult = compilation.Emit(ms);
                    var result = new CompilerResult();

                    if (compilationResult.Success)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        result.Assembly = ms.ToArray();                        
                    }
                    else
                    {
                        result.Errors.AddRange(compilationResult.Diagnostics.Select(s => s.ToString()));
                    }
                    return Task.FromResult(result);
                }
            }
            catch (Exception ex)
            {
                throw new CompilerException(ex.Message, ex);
            }
        }
    }
}
