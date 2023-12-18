#region Solution Code
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMTest.DataSource;
using System.Runtime.Caching;

namespace NMTest.Test;

[TestClass]
public class CacheTests
{
    [TestMethod]
    public void NullKey_Read()
    {
        string testKey = "testKey";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;
        
        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        object result = ds.GetValue(testKey);

        Assert.IsNull(result);
    }

    //Check that a read from the database can be performed if the local cache and distributed cache don't have the given key.
    [TestMethod]
    public void DatabaseKey_Read()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        db.StoreValue(testKey, testValue);

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        string result = ds.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that an empty distributed cache key is populated by the database value when a read occurs.
    [TestMethod]
    public void DatabaseKey_DistributedCache_Read()
    {
        string testKey = "testKey1";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        db.StoreValue(testKey, testValue);

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        ds.GetValue(testKey).ToString();

        string result = distributedCache.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that an empty local cache key is populated by the database value when a read occurs.
    [TestMethod]
    public void DatabaseKey_LocalCache_Read()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        db.StoreValue(testKey, testValue);

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        ds.GetValue(testKey).ToString();

        string result = localCache.Get(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that a read from the distributed cache can be performed if the local cache and database don't have the given key.
    [TestMethod]
    public void DistributeCachedKey_Read()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        distributedCache.StoreValue(testKey, testValue);

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        string result = ds.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that an empty local cache key is populated by the database value when a read occurs.
    [TestMethod]
    public void DistributeCachedKey_LocalCache_Read()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        distributedCache.StoreValue(testKey, testValue);

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        ds.GetValue(testKey).ToString();

        string result = localCache.Get(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that a read from the distributed cache can be performed if the local cache and database don't have the given key.
    [TestMethod]
    public void LocalCacheKey_Read()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        localCache.Set(new CacheItem(testKey, testValue), new CacheItemPolicy() { SlidingExpiration = new TimeSpan(1, 0, 0) });

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        string result = ds.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that a datasource write correctly populates the local cache.
    [TestMethod]
    public void LocalCacheKey_Write()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;
        
        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        ds.StoreValue(testKey, testValue);

        string result = localCache.Get(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that a datasource write correctly populates the distributed cache.
    [TestMethod]
    public void DistributedCacheKey_Write()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        ds.StoreValue(testKey, testValue);

        string result = distributedCache.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that a datasource write correctly populates the database.
    [TestMethod]
    public void DatabaseKey_Write()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        ds.StoreValue(testKey, testValue);

        string result = db.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    //Check that a datasource write and read have matching values.
    [TestMethod]
    public void DataSourceKey_ReadWrite()
    {
        string testKey = "testKey";
        string testValue = "testValue";

        DatabaseStore db = new DatabaseStore();
        DistributedCacheStore distributedCache = new DistributedCacheStore();
        ObjectCache localCache = MemoryCache.Default;

        CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);

        ds.StoreValue(testKey, testValue);

        string result = ds.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }
}
#endregion