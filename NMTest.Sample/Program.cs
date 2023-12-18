using NMTest.DataSource;
using System;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading;

#region Solution code
//Declare variables
DatabaseStore db = new DatabaseStore();
DistributedCacheStore distributedCache = new DistributedCacheStore();
ObjectCache localCache = MemoryCache.Default;
Random rnd = new Random();

//Initialise the values in the database. The local cache and distributed cache should start empty.
db.StoreValue("key0", "value0");
db.StoreValue("key1", "value1");
db.StoreValue("key2", "value2");
db.StoreValue("key3", "value3");
db.StoreValue("key4", "value4");
db.StoreValue("key5", "value5");
db.StoreValue("key6", "value6");
db.StoreValue("key7", "value7");
db.StoreValue("key8", "value8");
db.StoreValue("key9", "value9");

CachingDataSource ds = new CachingDataSource(db, distributedCache, localCache);
#endregion

for (var i = 0; i < 10; i++)
{
    new Thread(() =>
    {
#region Solution code
        string request = string.Empty;
        string response = string.Empty;
        Stopwatch sw = new Stopwatch();
#endregion

        for (var j = 0; j < 50; j++)
        {
#region Solution code
            request = "key" + rnd.Next(0, 9).ToString();
            
            sw.Start();
            response = ds.GetValue(request).ToString();
            sw.Stop();

            Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId.ToString() + "] Request " + "key" + request + ", response '" + response + "', time: " + sw.ElapsedMilliseconds.ToString() + "ms");
            sw.Reset();
#endregion
        }

    }).Start();
}
