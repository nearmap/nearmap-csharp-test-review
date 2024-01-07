using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMTest.DataSource;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Moq;

namespace NMTest.Test;

[TestClass]
public class TreadSafetyTests
{
    private readonly MockRepository _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IReadableDatabase> _databaseMock;
    private readonly Mock<IDistributedCacheStore> _distributedCacheMock;
    private readonly Mock<ObjectCache> _localCacheMock;

    private CachingDataSource _dataSource;
    
    private const string Key = "key";
    private const string Value = "value";
    private const int NumberOfThreads = 100;

    public TreadSafetyTests()
    {
        _databaseMock = _mockRepo.Create<IReadableDatabase>();
        _distributedCacheMock = _mockRepo.Create<IDistributedCacheStore>();
        _localCacheMock = _mockRepo.Create<ObjectCache>();
    }
    
    [TestInitialize]
    public void BeforeEachTest()
    {
        _databaseMock.Reset();
        _distributedCacheMock.Reset();
        _localCacheMock.Reset(); 
        _dataSource = new CachingDataSource(_databaseMock.Object, _distributedCacheMock.Object, _localCacheMock.Object);   
    }

    [TestMethod]
    public void WhenSeveralThreadsRetrieveSameValue_DistributedCacheAndDatabase_CalledOnlyOnce()
    {
        var firstNullRestValues = new string[] { null }.Concat(Enumerable.Repeat(Value, NumberOfThreads - 1).ToArray()).ToArray();
        // First call to Local Cache inside a critical section, while all other threads are blocked
        // First, return 'null' to force retrieval of value from down the food chain, then return 'value' for all remaining calls
        _localCacheMock.Setup(m => m.Get(It.Is<string>(s => s == Key), null))
            .Returns(new Queue<string>(firstNullRestValues).Dequeue)
            .Verifiable(Times.Exactly(NumberOfThreads));
        // The only DB call that is going to happen
        _databaseMock.Setup(m => m.GetValue(It.Is<string>(s => s == Key)))
            .Returns(Value)
            .Verifiable(Times.Exactly(1));
        // The only one Get call on the Distributed Cache call that's going to happen
        _distributedCacheMock.Setup(m => m.GetValue(It.Is<string>(s => s == Key)))
            .Returns(null)
            .Verifiable(Times.Exactly(1));
        // The only one StoreValue call on the Distributed Cache call that's going to happen
        _distributedCacheMock.Setup(m => m.StoreValue(
                It.Is<string>(s => s == Key), 
                It.Is<string>(s => s == Value)))
            .Verifiable(Times.Exactly(1));
        _localCacheMock.Setup(m => m.Set(It.Is<CacheItem>(i => i.Key == Key && i.Value.ToString() == Value), It.IsAny<CacheItemPolicy>()))
            .Verifiable(Times.Exactly(1));

        var tasks = new List<Task>(); 
        for (var i = 0; i < NumberOfThreads; i++)
        {
            var task = Task.Factory.StartNew(() =>
            {
                Assert.AreEqual(Value, _dataSource.GetValue(Key).ToString());
            });

            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());
        
        _mockRepo.VerifyAll();
    }

    [TestMethod]
    public void WhenDBReturnsNull_SaveAsSentinelValue_And_NeverCallDBAgain()
    {
        _localCacheMock.Setup(m => m.Get(It.Is<string>(s => s == Key), null))
            .Returns(new Queue<string>(new [] { null, DatabaseStore.NotFoundInDatabase }).Dequeue)
            .Verifiable(Times.Exactly(2));

        _databaseMock.Setup(m => m.GetValue(It.Is<string>(s => s == Key)))
            .Returns(null)
            .Verifiable(Times.Exactly(1));

        _distributedCacheMock.Setup(m => m.GetValue(It.Is<string>(s => s == Key)))
            .Returns(null)
            .Verifiable(Times.Exactly(1));
        
        _distributedCacheMock.Setup(m => m.StoreValue(
                It.Is<string>(s => s == Key), 
                It.Is<string>(s => s == DatabaseStore.NotFoundInDatabase)))
            .Verifiable(Times.Exactly(1));
        
        _localCacheMock.Setup(m => m.Set(
                It.Is<CacheItem>(i => i.Key == Key && i.Value.ToString() == DatabaseStore.NotFoundInDatabase), 
                It.IsAny<CacheItemPolicy>()))
            .Verifiable(Times.Exactly(1));

        Assert.IsNull(_dataSource.GetValue(Key));
        Assert.IsNull(_dataSource.GetValue(Key));
        
        _mockRepo.VerifyAll();
    }
}