using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;

namespace NMTest.DataSource
{
    /// <summary>
    /// Readonly Cached Data Source efficiently retrieving keyed values from DatabaseStore utilizing
    /// faster Distributed Cache Store shared across nodes and fastest Local Cache shared by worker threads.
    /// </summary>
    public class CachingDataSource : IDataSource
    {
        private static readonly ConcurrentDictionary<string, object> keyLocks = new ConcurrentDictionary<string, object>();
        
        public static readonly string ValueDoesNotExist = "VALUE DOES NOT EXIST AND NEVER WILL";
        public static bool DoesNotExist(object value) => $"{value}" == ValueDoesNotExist;

        private readonly IDatabaseStore database;
        private readonly IDistributedCacheStore distributedCache;
        private readonly ObjectCache localCache;

        public CachingDataSource(
            IDatabaseStore databaseStore, 
            IDistributedCacheStore distributedCacheStore,
            ObjectCache objectCache)
        {
            database = databaseStore ?? throw new ArgumentNullException(nameof(databaseStore));
            distributedCache = distributedCacheStore ?? throw new ArgumentNullException(nameof(distributedCacheStore));
            localCache = objectCache ?? throw new ArgumentNullException(nameof(objectCache));
        }

        public object GetValue(string key)
        {
            // Avoid unnecessary round trips for null or empty key, that will not match to any value anyway
            if (string.IsNullOrEmpty(key)) return null;

            // make sure we hit the remote database only once and all subsequent calls or other concurrent 
            // requests to the same key hit the caches instead, ensuring maximum performance and throughput
            var keyLock = keyLocks.GetOrAdd(key, (k) => new object());
            lock (keyLock)
            {
                // Attempt to get the value from the local cache first
                object value = localCache.Get(key);

                if (value == null)
                {
                    // Attempt to get the value from distributed cache
                    value = distributedCache.GetValue(key);

                    if (value == null)
                    {
                        // Attempt to get the value from the database
                        value = database.GetValue(key) ?? ValueDoesNotExist;

                        // Populate the distributed cache with the value
                        distributedCache.StoreValue(key, value);
                    }

                    localCache.Set(new CacheItem(key, value), new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) });
                }

                return DoesNotExist(value) ? null : value;
            }
        }
    }
}