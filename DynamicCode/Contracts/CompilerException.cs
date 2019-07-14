using System;

namespace Contracts
{
    public class CompilerException : Exception
    {
        public CompilerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
