using System.Collections.Generic;
using System.Threading;

namespace NMTest.DataSource
{
    public class DistributedCacheStore : IDistributedCacheStore
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public object GetValue(string key)
        {
            //simulates 100 ms roundtrip to the distributed cache
            Thread.Sleep(100);
            return _values.TryGetValue(key, out object value) ? value : null;
        }

        public void StoreValue(string key, object value)
        {
            //simulates 100 ms roundtrip to the distributed cache
            Thread.Sleep(100);
            _values[key] = value;
        }
    }
}
