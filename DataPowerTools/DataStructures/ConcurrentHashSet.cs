using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DataPowerTools.DataStructures
{
    internal class ConcurrentHashSet<T> : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public HashSet<T> Hashset { get; } = new HashSet<T>();

        public T[] ToArray() => Hashset.ToArray();
        public IEnumerable<T> AsEnumerable() => (IEnumerable<T>)Hashset;

        #region Implementation of ICollection<T> ...ish

        public bool Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return Hashset.Add(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                Hashset.Clear();
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return Hashset.Contains(item);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return Hashset.Remove(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return Hashset.Count;
                }
                finally
                {
                    if (_lock.IsReadLockHeld) _lock.ExitReadLock();
                }
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (_lock != null)
                    _lock.Dispose();
        }

        ~ConcurrentHashSet()
        {
            Dispose(false);
        }

        #endregion
    }
}