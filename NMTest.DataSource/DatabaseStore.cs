using System.Collections.Concurrent;
using System.Threading;

namespace NMTest.DataSource
{
    public class DatabaseStore : IDatabaseStore
    {
        private readonly ConcurrentDictionary<string, object> _values = new ConcurrentDictionary<string, object>();

        public object GetValue(string key)
        {
            //simulates 500 ms roundtrip to the database
            Thread.Sleep(500);
            return _values.TryGetValue(key, out object value) ? value : null;
        }

        public void StoreValue(string key, object value)
        {
            //simulates 500 ms roundtrip to the database
            Thread.Sleep(500);
            _values[key] = value;
        }
    }
}