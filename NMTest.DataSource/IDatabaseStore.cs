namespace NMTest.DataSource
{
    public interface IDatabaseStore
    {
        object GetValue(string key);
        void StoreValue(string key, object value);
    }
}