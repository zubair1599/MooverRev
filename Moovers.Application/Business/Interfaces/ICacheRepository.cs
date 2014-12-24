namespace Business.Repository.Models
{
    public interface ICacheRepository
    {
        void Store(string key, object value);
        bool Contains(string key);
        object Get(string key);
        void Clear(string containing = null);
        T Get<T>(string key);
    }
}