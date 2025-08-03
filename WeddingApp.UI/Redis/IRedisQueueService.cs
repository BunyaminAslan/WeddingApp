namespace WeddingApp.UI.Redis
{
    public interface IRedisQueueService
    {
        Task EnqueueAsync(string key, string value);
        //Task<string?> DequeueAsync(string key);

        Task<List<string>> DequeueBatchAsync(string queueName, int batchSize);

    }
}
