using NMTest.DataSource;
using System;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading;

var database = new DatabaseStore();
var distributedCache = new DistributedCacheStore();
var localCache = MemoryCache.Default;
var rnd = new Random();

// Initialise the values in the database. The local cache and distributed cache should start empty.
for (int i = 0; i < 10; i++)
{
    database.StoreValue($"key{i}", $"value{i}");
}

var dataSource = new CachingDataSource(database, distributedCache, localCache);

for (var i = 0; i < 10; i++)
{
    // note: TPL could be used there, but each Task may not necessarily run in a separate thread
    //       while the requirement is explicit about exactly 10 threads, so stickign to that
    new Thread(() =>
    {
        var stopWatch = new Stopwatch();
        var threadId = Environment.CurrentManagedThreadId;

        for (var j = 0; j < 50; j++)
        {
            var request = $"key{rnd.Next(0, 10)}"; // fix: random int between 0 and 9, inclusive, requires maxValue=10

            stopWatch.Start();
            var response = $"{dataSource.GetValue(request)}";
            stopWatch.Stop();

            double elapsed = stopWatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"[{threadId}] Request '{request}', response '{response}', time: {elapsed:F2} ms");
            stopWatch.Reset();
        }
    }).Start();
}