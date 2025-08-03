using WeddingApp.UI.Cache;
using WeddingApp.UI.Models;

namespace WeddingApp.UI.Redis
{
    public class FallbackQueueService : IUploadQueue
    {
        private readonly IRedisQueueService _redisQueue;
        private readonly IUploadQueue _memoryQueue;

        public int Count => throw new NotImplementedException();

        public FallbackQueueService(IRedisQueueService redisQueue, IUploadQueue memoryQueue)
        {
            _redisQueue = redisQueue;
            _memoryQueue = memoryQueue;
        }

        public async Task EnqueueAsync(string queueName, string value)
        {
            try
            {
                await _redisQueue.EnqueueAsync(queueName, value);
            }
            catch
            {
                await _memoryQueue.EnqueueAsync(queueName, value);
            }
        }

        public async Task<List<string>> DequeueBatchAsync(string queueName, int batchSize)
        {
            try
            {
                var items = await _redisQueue.DequeueBatchAsync(queueName, batchSize);
                if (items.Count > 0) return items;

                // Redis boşsa MemoryQueue’dan çek
                return await _memoryQueue.DequeueBatchAsync(queueName, batchSize);
            }
            catch
            {
                return await _memoryQueue.DequeueBatchAsync(queueName, batchSize);
            }
        }

        public void Enqueue(CachedUpload item)
        {
            throw new NotImplementedException();
        }

        public List<CachedUpload> DequeueBatch(int maxCount)
        {
            throw new NotImplementedException();
        }
    }
}
