#region Solution Code
using System;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMTest.DataSource;
using System.Runtime.Caching;

namespace NMTest.Test;

[TestClass]
public class CacheTests
{
    private DatabaseStore _db;
    private DistributedCacheStore _distributedCache;
    private ObjectCache _localCache;
    private CachingDataSource _ds;

    private const string TestKey = "testKey";
    private const string TestValue = "testValue";

    private void StoreValueEverywhere(string key, string value)
    {
        _db.StoreValue(key, value);
        _distributedCache.StoreValue(key, value);
        _localCache.Set(new CacheItem(key, value), new CacheItemPolicy());
    }
    
    [TestInitialize]
    public void BeforeEachTest()
    {
        _db = new DatabaseStore();
        _distributedCache = new DistributedCacheStore();
        _localCache = new MemoryCache(Guid.NewGuid().ToString(), new NameValueCollection());
        
        _ds = new CachingDataSource(_db, _distributedCache, _localCache);   
    }
    
    [TestMethod]
    public void NullKey_Read()
    {
        var result = _ds.GetValue(TestKey);

        Assert.IsNull(result);
    }

    //Check that a read from the database can be performed if the local cache and distributed cache don't have the given key.
    [TestMethod]
    public void DatabaseKey_Read()
    {
        _db.StoreValue(TestKey, TestValue);

        var result = _ds.GetValue(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that an empty distributed cache key is populated by the database value when a read occurs.
    [TestMethod]
    public void DatabaseKey_DistributedCache_Read()
    {
        _db.StoreValue(TestKey, TestValue);

        _ds.GetValue(TestKey);

        var result = _distributedCache.GetValue(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that an empty local cache key is populated by the database value when a read occurs.
    [TestMethod]
    public void DatabaseKey_LocalCache_Read()
    {
        _db.StoreValue(TestKey, TestValue);
        _ds.GetValue(TestKey);

        var result = _localCache.Get(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that a read from the distributed cache can be performed if the local cache and database don't have the given key.
    [TestMethod]
    public void DistributeCachedKey_Read()
    {
        _distributedCache.StoreValue(TestKey, TestValue);
        
        var result = _ds.GetValue(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that an empty local cache key is populated by the database value when a read occurs.
    [TestMethod]
    public void DistributeCachedKey_LocalCache_Read()
    {
        _distributedCache.StoreValue(TestKey, TestValue);

        _ds.GetValue(TestKey);

        var result = _localCache.Get(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that a read from the distributed cache can be performed if the local cache and database don't have the given key.
    [TestMethod]
    public void LocalCacheKey_Read()
    {
        _localCache.Set(new CacheItem(TestKey, TestValue), new CacheItemPolicy() { SlidingExpiration = new TimeSpan(1, 0, 0) });

        var result = _ds.GetValue(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that a datasource write correctly populates the local cache.
    [TestMethod]
    public void LocalCacheKey_Write()
    {
        StoreValueEverywhere(TestKey, TestValue);

        var result = _localCache.Get(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that a datasource write correctly populates the distributed cache.
    [TestMethod]
    public void DistributedCacheKey_Write()
    {
        StoreValueEverywhere(TestKey, TestValue);

        var result = _distributedCache.GetValue(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that a datasource write correctly populates the database.
    [TestMethod]
    public void DatabaseKey_Write()
    {
        StoreValueEverywhere(TestKey, TestValue);

        var result = _db.GetValue(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }

    //Check that a datasource write and read have matching values.
    [TestMethod]
    public void DataSourceKey_ReadWrite()
    {
        StoreValueEverywhere(TestKey, TestValue);

        var result = _ds.GetValue(TestKey).ToString();

        Assert.AreEqual(TestValue, result);
    }
}
#endregion