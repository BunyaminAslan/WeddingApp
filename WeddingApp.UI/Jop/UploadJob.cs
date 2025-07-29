using Wedding.Model.DB;
using Wedding.Repository.Interfaces;
using WeddingApp.UI.Cache;

namespace WeddingApp.UI.Jop
{
    public class UploadJob : IHostedService
    {
        private readonly IUploadQueue _queue;
        private readonly IServiceProvider _sp;
        private Timer _timer;
        private bool isLock = false;
        public UploadJob(IUploadQueue queue, IServiceProvider sp)
        {
            _queue = queue;
            _sp = sp;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Execute, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async void Execute(object state)
        {
            if (isLock) return;

            isLock = true;
            Console.WriteLine("--JOP STARTED--");
            Console.WriteLine($"queue item count : {_queue.Count}");

            var items = _queue.DequeueBatch(100);
            if (!items.Any())
            {
                isLock = false; return;
            }

            Console.WriteLine("Get 100 item");

            using var scope = _sp.CreateScope();
            var cloudinary = scope.ServiceProvider.GetRequiredService<CloudinaryService>();
            var photoService = scope.ServiceProvider.GetRequiredService<IPhotoRepository>();

            foreach (var item in items)
            {
                try
                {
                    var bytes = Convert.FromBase64String(item.Base64);
                    using var ms = new MemoryStream(bytes);

                    var url = await cloudinary.UploadImageAsync(ms, item.FileName);
                    var ext = Path.GetExtension(item.FileName)?.TrimStart('.').ToLowerInvariant() ?? "jpg";
                    if (ext == "heic") ext = "jpg";

                    await photoService.AddAsync(new PhotoDb
                    {
                        PublicId = url,
                        Extension = ext,
                        Ip = item.Ip,
                        Device = item.Device
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR!!{item.Base64} - {ex.Message}");
                }
            }

            await photoService.SaveAsync();

            Console.WriteLine("-- Successfully Done --");
            isLock = false;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }
    }
}
