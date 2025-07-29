using System.Collections.Concurrent;
using WeddingApp.UI.Controllers;
using static WeddingApp.UI.Controllers.UploadController;

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
        public int Count => _queue.Count;

    }
}
