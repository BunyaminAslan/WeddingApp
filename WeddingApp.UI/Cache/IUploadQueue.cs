using WeddingApp.UI.Models;

namespace WeddingApp.UI.Cache
{
    public interface IUploadQueue
    {
        void Enqueue(CachedUpload item);

        List<CachedUpload> DequeueBatch(int maxCount);
        int Count { get; }

        Task EnqueueAsync(string key, string value);

        Task<List<string>> DequeueBatchAsync(string queueName, int batchSize);

    }
}
