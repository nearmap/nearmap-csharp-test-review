namespace NMTest.DataSource
{
    public interface IDistributedCacheStore
    {
        object GetValue(string key);
        void StoreValue(string key, object value);
    }
}