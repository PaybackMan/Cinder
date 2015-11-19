using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public class CacheProvider
    {
        static Dictionary<Type, ICacheQueries> _caches = new Dictionary<Type, ICacheQueries>();
        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        public static void Register<TRepository, TQueryCache>()
            where TRepository : IRepository
            where TQueryCache : ICacheQueries, new()
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                ICacheQueries cache;
                if (_caches.TryGetValue(typeof(TRepository), out cache)
                    && !cache.GetType().Equals(typeof(TQueryCache)))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _caches[typeof(TRepository)] = Activator.CreateInstance<TQueryCache>();
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                else if (cache == null)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _caches.Add(typeof(TRepository), Activator.CreateInstance<TQueryCache>());
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public static ICacheQueries Get<TRepository>()
            where TRepository : IRepository
        {
            _lock.EnterReadLock();
            try
            {
                ICacheQueries cache;
                _caches.TryGetValue(typeof(TRepository), out cache);
                return cache;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
