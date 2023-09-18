using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.Infrastructure
{
    public sealed class WriteLockDisposable : IDisposable
    {
        // Fields
        private readonly ReaderWriterLockSlim _rwLock;

        // Methods
        public WriteLockDisposable(ReaderWriterLockSlim rwLock)
        {
            this._rwLock = rwLock;
        }

        void IDisposable.Dispose()
        {
            this._rwLock.ExitWriteLock();
        }
    }
}
