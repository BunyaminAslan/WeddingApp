using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WeddingApp.UI.Cache;
using WeddingApp.UI.Models;

namespace WeddingApp.UI.Redis
{
    public class FallbackQueueService : IUploadQueue
    {
        private readonly IUploadQueue _redisQueue;
        private readonly IUploadQueue _memoryQueue;


        public FallbackQueueService(/*IRedisQueueService redisQueue,*/ IUploadQueue memoryQueue, IUploadQueue redisQueue)
        {
            //_redisQueue = redisQueue;
            _memoryQueue = memoryQueue;
            _redisQueue = redisQueue;
        }

        public int Count => _redisQueue.Count == 0 ? _memoryQueue.Count : _redisQueue.Count;

        public async Task EnqueueAsync(string queueName, string value)
        {

            try
            {
                await _redisQueue.EnqueueAsync(queueName, value);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Redis yazım hatası: {ex.Message}, Memory'e alındı.");


                await _memoryQueue.EnqueueAsync(queueName, value);
            }
        }

        public async Task<List<string>> DequeueBatchAsync(string queueName, int batchSize)
        {

            try
            {
                return await _redisQueue.DequeueBatchAsync(queueName, batchSize);
            }
            catch (Exception ex)
            {

                Console.WriteLine($" Redis okuma hatası: {ex.Message}, Memory'den okunuyor.");

                return await _memoryQueue.DequeueBatchAsync(queueName, batchSize);
            }

        }

        public void Enqueue(CachedUpload item) => _memoryQueue.Enqueue(item);

        public List<CachedUpload> DequeueBatch(int maxCount) => _memoryQueue.DequeueBatch(maxCount);

    }
}
