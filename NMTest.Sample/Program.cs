using NMTest.DataSource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

#region Solution code

var db = new DatabaseStore();
var distributedCache = new DistributedCacheStore();
var localCache = MemoryCache.Default;
var rnd = new Random();

//Initialise the values in the database. The local cache and distributed cache should start empty.
for (var i = 0; i < 10; i++)
{
    db.StoreValue($"key{i}", $"value{i}");
}

var ds = new CachingDataSource(db, distributedCache, localCache);
#endregion

var tasks = new List<Task>();
for (var i = 0; i < 10; i++)
{
    var task = Task.Factory.StartNew(() =>
    {
        #region Solution code

        var sw = new Stopwatch();

        #endregion

        for (var j = 0; j < 50; j++)
        {
            #region Solution code

            var request = $"key{rnd.Next(0, 9)}";

            sw.Start();
            var response = ds.GetValue(request).ToString();
            sw.Stop();

            // See https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1840
            // for Managed Thread Id
            Console.WriteLine(
                $"[{Environment.CurrentManagedThreadId}] Request key: '{request}', response: '{response}', time: {sw.ElapsedMilliseconds}ms");
            sw.Reset();

            #endregion
        }
    });
    tasks.Add(task);
}

Task.WaitAll(tasks.ToArray());