using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Contracts
{
    public abstract class Compiler: IDisposable
    {
        protected readonly CompilerOptions Options;
        private bool _disposed;

        protected Compiler(CompilerOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _disposed = false;
        }

        public abstract Task<CompilerResult> CompileAsync(string code);

        protected virtual void DisposeCore(bool disposing)
        {            
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                DisposeCore(disposing);
                // Note disposing has been done.
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        ~Compiler()
        {
            Dispose(false);
        }
    }
}
