using RedisApp.Data;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisApp.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _cache;
        public CacheService()
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            _cache = redis.GetDatabase();

        }
        public T GetData<T>(string key)
        {
            var value = _cache.StringGet(key);
            if (!string.IsNullOrEmpty(value))
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }

        public object RemoveData(string key)
        {
            var _exist = _cache.KeyExists(key);
            if (_exist)
            {
                return _cache.KeyDelete(key);
            }
            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            return _cache.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
        }
    }
}
