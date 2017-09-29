using System;
using System.Collections.Generic;
using System.Threading;

namespace Enferno.Public.Caching
{
    public interface ICacheKeyLock
    {
        IDisposable AcquireLock(string key);
    }

    public class CacheKeyNoLock : ICacheKeyLock
    {
        public IDisposable AcquireLock(string key)
        {
            return null;
        }
    }

    public class CacheKeyLock : ICacheKeyLock
    {
        private readonly Dictionary<string, LockObject> keyLocks = new Dictionary<string, LockObject>();
        private readonly object keyLocksLock = new object();

        public IDisposable AcquireLock(string key)
        {
            LockObject obj;
            lock (keyLocksLock)
            {
                if (!keyLocks.TryGetValue(key, out obj))
                {
                    keyLocks[key] = obj = new LockObject(key);
                }
                obj.Withdraw();
            }
            Monitor.Enter(obj);
            return new DisposableToken(this, obj);
        }

        private void ReturnLock(DisposableToken disposableLock)
        {
            var obj = disposableLock.LockObject;
            lock (keyLocksLock)
            {
                if (obj.Return())
                {
                    keyLocks.Remove(obj.Key);
                }
                Monitor.Exit(obj);
            }
        }

        private class DisposableToken : IDisposable
        {
            private readonly CacheKeyLock keyLock;
            private bool disposed;

            public DisposableToken(CacheKeyLock stringLock, LockObject lockObject)
            {
                keyLock = stringLock;
                LockObject = lockObject;
            }

            public LockObject LockObject { get; }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~DisposableToken()
            {
                Dispose(false);
            }

            private void Dispose(bool disposing)
            {
                if (disposing && !disposed)
                {
                    keyLock.ReturnLock(this);
                    disposed = true;
                }
            }
        }

        private class LockObject
        {
            private int leaseCount;

            public LockObject(string key)
            {
                Key = key;
            }

            public string Key { get; }

            public void Withdraw()
            {
                Interlocked.Increment(ref leaseCount);
            }

            public bool Return()
            {
                return Interlocked.Decrement(ref leaseCount) == 0;
            }
        }
    }
}
