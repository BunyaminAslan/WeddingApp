using System.Collections.Concurrent;
using WeddingApp.UI.Models;

namespace WeddingApp.UI.Cache
{
    public class UploadQueue : IUploadQueue
    {
        private readonly ConcurrentQueue<CachedUpload> _queue = new();


        public void Enqueue(CachedUpload item) => _queue.Enqueue(item);

        public List<CachedUpload> DequeueBatch(int maxCount)
        {
            var items = new List<CachedUpload>();
            while (_queue.TryDequeue(out var item) && items.Count < maxCount)
                items.Add(item);
            return items;
        }
        public int Count => _queue.Count == 0 ? _squeue.Count : _queue.Count;


        //string queue for redis fallback
        private readonly ConcurrentQueue<string> _squeue = new();

        public Task EnqueueAsync(string queueName, string value)
        {
            _squeue.Enqueue(value);
            return Task.CompletedTask;
        }

        public Task<List<string>> DequeueBatchAsync(string queueName, int batchSize)
        {
            var list = new List<string>();

            for (int i = 0; i < batchSize && _squeue.TryDequeue(out var item); i++)
            {
                list.Add(item);
            }

            return Task.FromResult(list);
        }
    }
}
