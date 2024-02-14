using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMTest.DataSource;
using System;
using System.Runtime.Caching;

namespace NMTest.Test;

[TestClass]
public class CacheTests
{
    private const string testKey = "testKey";
    private const string testValue = "testValue";
    IDatabaseStore database;
    IDistributedCacheStore distributedCache;
    ObjectCache localCache;
    IDataSource dataSource;

    [TestInitialize]
    public void Test_Initialize()
    {
        database = new DatabaseStore();
        distributedCache = new DistributedCacheStore();
        // issue: MemoryCache.Default is effectively a singleton that persists across tests and contaminates them
        // - https://learn.microsoft.com/en-us/dotnet/api/system.runtime.caching.memorycache.default
        // fix: isolate each test with a dedicated instance of MemoryCache and dispose of it upon test completion
        localCache = new MemoryCache(name: $"{Guid.NewGuid()}");
        dataSource = new CachingDataSource(database, distributedCache, localCache);
    }

    [TestCleanup]
    public void Test_Cleanup()
    {
        database = null;
        distributedCache = null;
        (localCache as MemoryCache)?.Dispose();
        localCache = null;
        dataSource = null;
    }


    /// <summary>
    /// extend: additional data driven tests for null argument exceptions for null dependencies when creating CachingDataSource
    /// </summary>
    /// <param name="databaseStoreIsNull"></param>
    /// <param name="distributedCacheStoreIsNull"></param>
    /// <param name="objectCacheIsNull"></param>
    [TestMethod]
    [DataRow(false, true, true)]
    [DataRow(true, false, true)]
    [DataRow(true, true, false)]
    [DataRow(false, false, true)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [ExpectedException(typeof(ArgumentNullException), "CachingDataSource store dependency cannot be null")]
    public void NullDependency_CachingDataSource(
        bool databaseStoreIsNull,
        bool distributedCacheStoreIsNull,
        bool objectCacheIsNull)
    {
        IDatabaseStore databaseStore = databaseStoreIsNull ? null : database;
        IDistributedCacheStore distributedCacheStore = distributedCacheStoreIsNull ? null : distributedCache;
        ObjectCache objectCache = objectCacheIsNull ? null : localCache;

        new CachingDataSource(databaseStore, distributedCacheStore, objectCache);
    }

    /// <summary>
    /// Check that a null or empty key returns a null result
    /// </summary>
    /// <param name="key"></param>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void NullKey_Read(string key)
    {
        object result = dataSource.GetValue(key);

        Assert.IsNull(result);
    }

    /// <summary>
    /// Check that a read with a key that does not exist in the store returns a null result
    /// </summary>
    [TestMethod]
    public void UnmatchedKey_Read()
    {
        object result = dataSource.GetValue(testKey);

        Assert.IsNull(result);
    }

    /// <summary>
    /// Check that a read from the database can be performed if the local cache and distributed cache don't have the given key.
    /// </summary>
    [TestMethod]
    public void DatabaseKey_Read()
    {
        database.StoreValue(testKey, testValue);

        string result = dataSource.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Check that an empty distributed cache key is populated by the database value when a read occurs.
    /// </summary>
    [TestMethod]
    public void DatabaseKey_DistributedCache_Read()
    {
        database.StoreValue(testKey, testValue);

        dataSource.GetValue(testKey).ToString();

        string result = distributedCache.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Check that an empty local cache key is populated by the database value when a read occurs.
    /// </summary>
    [TestMethod]
    public void DatabaseKey_LocalCache_Read()
    {
        database.StoreValue(testKey, testValue);

        dataSource.GetValue(testKey).ToString();

        string result = localCache.Get(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Check that a read from the distributed cache can be performed if the local cache and database don't have the given key.
    /// </summary>
    [TestMethod]
    public void DistributeCachedKey_Read()
    {
        distributedCache.StoreValue(testKey, testValue);

        string result = dataSource.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Check that an empty local cache key is populated by the database value when a read occurs.
    /// </summary>
    [TestMethod]
    public void DistributeCachedKey_LocalCache_Read()
    {
        distributedCache.StoreValue(testKey, testValue);

        dataSource.GetValue(testKey).ToString();

        string result = localCache.Get(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Check that a read from the distributed cache can be performed if the local cache and database don't have the given key.
    /// </summary>
    [TestMethod]
    public void LocalCacheKey_Read()
    {
        localCache.Set(new CacheItem(testKey, testValue), new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) });

        string result = dataSource.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Check that a distributed cache write correctly populates the distributed cache.
    /// </summary>
    [TestMethod]
    public void DistributedCacheKey_Write()
    {
        distributedCache.StoreValue(testKey, testValue);

        string result = distributedCache.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }

    /// <summary>
    /// Check that a database write correctly populates the database.
    /// </summary>
    [TestMethod]
    public void DatabaseKey_Write()
    {
        database.StoreValue(testKey, testValue);

        string result = database.GetValue(testKey).ToString();

        Assert.AreEqual(testValue, result);
    }
}