using System.Collections.Generic;
using System.Threading;

namespace NMTest.DataSource
{
    public class DatabaseStore
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public object GetValue(string key)
        {
            //simulates 500 ms roundtrip to the database
            Thread.Sleep(500);
            object value;
            if (_values.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        public void StoreValue(string key, object value)
        {
            //simulates 500 ms roundtrip to the database
            Thread.Sleep(500);
            _values[key] = value;
        }
    }
}