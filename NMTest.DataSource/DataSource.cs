#region Solution code
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace NMTest.DataSource
{
    //I have introduced a local cache to improve performance for frequently requested items.
    public class CachingDataSource : IDataSource
    {
        private DatabaseStore database = new DatabaseStore();
        private DistributedCacheStore distributedCache = new DistributedCacheStore();
        private ObjectCache localCache = MemoryCache.Default;

        public CachingDataSource(
            DatabaseStore databaseStore,
            DistributedCacheStore distributedCacheStore,
            ObjectCache objectCache)
        {
            database = databaseStore;
            distributedCache = distributedCacheStore;
            localCache = objectCache;
        }
        
        public object GetValue(string key)
        {
            object value;

            //Attempt to get the value from the local cache first
            value = localCache.Get(key);

            if (value == null)
            {
                //Attempt to get the value from distributed cache
                value = distributedCache.GetValue(key);

                if (value == null)
                {
                    //Attempt to get the value from the database
                    value = database.GetValue(key);

                    //Populate the distributed cache with the value
                    distributedCache.StoreValue(key, value);
                }

                //Populate the local cache with the value so long as the value isn't NULL
                if (value != null)
                {
                    localCache.Set(new CacheItem(key, value), new CacheItemPolicy() { SlidingExpiration = new TimeSpan(1, 0, 0) });
                }
            }

            return value;
        }

        //Store new values in the database, distributed cache and local cache.
        public void StoreValue(string key, string value)
        {
            database.StoreValue(key, value);

            distributedCache.StoreValue(key, value);
            
            localCache.Set(new CacheItem(key, value), new CacheItemPolicy() { SlidingExpiration = new TimeSpan(1, 0, 0) });
        }
    }
}
#endregion