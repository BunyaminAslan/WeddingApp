using WeddingApp.UI.Cache;
using WeddingApp.UI.Models;

namespace WeddingApp.UI.Redis
{
    public class FallbackQueueService : IUploadQueue
    {
        //private readonly IRedisQueueService _redisQueue;
        private readonly IUploadQueue _queue;

        public int Count => _queue.Count;

        public FallbackQueueService(/*IRedisQueueService redisQueue,*/ IUploadQueue memoryQueue)
        {
            //_redisQueue = redisQueue;
            _queue = memoryQueue;
        }

        public async Task EnqueueAsync(string queueName, string value)
        {

            await _queue.EnqueueAsync(queueName, value);

        }

        public async Task<List<string>> DequeueBatchAsync(string queueName, int batchSize)
        {

            return await _queue.DequeueBatchAsync(queueName, batchSize);

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
