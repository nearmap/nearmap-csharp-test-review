using System.Collections.Generic;
using System.Threading;

namespace NMTest.DataSource
{
    public class DatabaseStore : IReadableDatabase
    {
        public static readonly string NotFoundInDatabase = "OMG SENTINEL VALUE BAD BAD NEVER USE ONE OH WELL WE'VE GOT TO USE SOMETHING!11";
        
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public object GetValue(string key)
        {
            //simulates 500 ms roundtrip to the database
            Thread.Sleep(500);
            return _values.TryGetValue(key, out var value) ? value : NotFoundInDatabase;
        }

        public void StoreValue(string key, object value)
        {
            //simulates 500 ms roundtrip to the database
            Thread.Sleep(500);
            _values[key] = value;
        }
    }
}