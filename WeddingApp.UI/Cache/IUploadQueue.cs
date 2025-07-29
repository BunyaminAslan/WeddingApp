using static WeddingApp.UI.Controllers.UploadController;

namespace WeddingApp.UI.Cache
{
    public interface IUploadQueue
    {
        void Enqueue(CachedUpload item);
        List<CachedUpload> DequeueBatch(int maxCount);
        int Count { get; }

    }
}
