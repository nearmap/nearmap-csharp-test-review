#region Solution code

using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;

namespace NMTest.DataSource
{
    /// <summary>
    /// Retrieves requested value from local cache, if not available, tries to retrieve from Distributed Cache,
    /// then the database. Once obtained or if failed to do so, caches the result for the duration of app domain
    /// lifetime. Thread-Safe. 
    /// </summary>
    public class CachingDataSource : IDataSource
    {
        private readonly IReadableDatabase _readableDatabase;
        private readonly IDistributedCacheStore distributedCache;
        private readonly ObjectCache localCache;
        private static readonly ConcurrentDictionary<string, object> keyLocks = new ConcurrentDictionary<string, object>();

        public CachingDataSource(
            IReadableDatabase readableDatabase,
            IDistributedCacheStore distributedCacheStore,
            ObjectCache objectCache)
        {
            _readableDatabase = readableDatabase;
            distributedCache = distributedCacheStore;
            localCache = objectCache;
        }

        private static object GetValueOrNull(object value)
        {
            return value.ToString() == DatabaseStore.NotFoundInDatabase ? null : value;
        }
        
        public object GetValue(string key)
        {
            //
            // We want to be locking the read access to the whole Local Cache -> Distributed Cache -> DB 
            // chain on per-key basis, and we do want to avoid locking the whole thing for every operation,
            // thus creating a bottleneck in the multi-threaded system.
            //
            // Thus, when there are 100s of threads simultaneously asking for the coordinates of Wooloomooloo
            // (because the marketing department, in their infinite wisdom, decided to provide free access to
            // high-res aerial photography of the suburb as a part of the most recent promo, and naturally,
            // people are keen to see what's in their neighbours backyards, flocking to the website, all hitting
            // same the endpoint with the same params.
            //
            // So when that happens, we don't want to dispatch all those same requests to the LC->DC->DB chain, but
            // but we do want to make them wait, on the per key basis, for the very first lucky request that managed
            // to travel there and bring back the good news, i.e. the value
            //
            var keyLock = keyLocks.GetOrAdd(key, (_) => new object());
            lock (keyLock)
            {
                var value = localCache.Get(key);

                if (value != null)
                    return GetValueOrNull(value);
                
                value = distributedCache.GetValue(key);

                if (value == null)
                {
                    // Not all nulls are created equal - we want to record the fact that value was not found in the DB,
                    // so we never have to traverse the long LC->DC->DB chain ever again
                    value = _readableDatabase.GetValue(key) ?? DatabaseStore.NotFoundInDatabase;
                    distributedCache.StoreValue(key, value);
                }

                localCache.Set(new CacheItem(key, value), new CacheItemPolicy());

                return GetValueOrNull(value);
            }
        }
    }
}
#endregion