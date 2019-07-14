namespace Contracts
{
    public class CompilerOptions
    {
        public string AssemblyName { get; set; }
        public string[] ReferencesAssemblyLocations { get; set; }
        public OutputType OutputType { get; set; }
        public string TemporaryCompilationLocation { get; set; }
    }
}
