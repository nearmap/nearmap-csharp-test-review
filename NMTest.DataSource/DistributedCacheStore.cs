using System.Collections.Generic;
using System.Threading;

namespace NMTest.DataSource
{
    public class DistributedCacheStore
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        
        public object GetValue(string key)
        {
            //simulates 100 ms roundtrip to the distributed cache
            Thread.Sleep(100);
            object value;
            if (_values.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        public void StoreValue(string key, object value)
        {
            //simulates 100 ms roundtrip to the distributed cache
            Thread.Sleep(100);
            _values[key] = value;
        }
    }
}
