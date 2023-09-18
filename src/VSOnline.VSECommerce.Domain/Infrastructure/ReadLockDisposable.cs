﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.Infrastructure
{
    public sealed class ReadLockDisposable : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        public ReadLockDisposable(ReaderWriterLockSlim rwLock)
        {
            this._rwLock = rwLock;
        }

        void IDisposable.Dispose()
        {
            this._rwLock.ExitReadLock();
        }


    }
}
