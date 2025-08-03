using StackExchange.Redis;
using WeddingApp.UI.Cache;
using WeddingApp.UI.Models;

namespace WeddingApp.UI.Redis
{
    public class RedisQueueService : IRedisQueueService,IUploadQueue
    {
        private readonly IConnectionMultiplexer _redis;

        public int Count => throw new NotImplementedException();

        public RedisQueueService(IConnectionMultiplexer redis)
        {
            _redis = redis;

            if (_redis == null) return;
        }

        public async Task EnqueueAsync(string key, string value)
        {

            if(_redis is null) return;
            var db = _redis.GetDatabase();
            await db.ListRightPushAsync(key, value);
        }

        public async Task<string?> DequeueAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.ListLeftPopAsync(key);
        }
        public async Task<List<string>> DequeueBatchAsync(string queueName, int batchSize)
        {
            var db = _redis.GetDatabase();
            var results = new List<string>();

            for (int i = 0; i < batchSize; i++)
            {
                var item = await db.ListLeftPopAsync(queueName);
                if (item.HasValue)
                    results.Add(item.ToString());
                else
                    break; // Kuyruk boşsa çık
            }

            return results;
        }

        public void Enqueue(CachedUpload item)
        {
            throw new NotImplementedException("NOT USED");
        }

        public List<CachedUpload> DequeueBatch(int maxCount)
        {
            throw new NotImplementedException("NOT USED");
        }
    }
}
