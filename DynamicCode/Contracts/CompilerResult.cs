using System.Collections.Generic;

namespace Contracts
{
    public class CompilerResult
    {
        public CompilerResult()
        {
            Errors = new List<string>();
        }

        public byte[] Assembly { get; set; }
        public List<string> Errors { get; set; }

        public bool IsSuccessful() => Errors?.Count == 0;
    }
}
