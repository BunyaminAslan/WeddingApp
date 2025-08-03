using Newtonsoft.Json;
using System.Collections.Generic;
using Wedding.Model.DB;
using Wedding.Repository.Interfaces;
using WeddingApp.UI.Cache;
using WeddingApp.UI.Models;
using WeddingApp.UI.Redis;

namespace WeddingApp.UI.Jop
{
    public class UploadJob : IHostedService
    {
        private readonly IUploadQueue _queue;


        private readonly IServiceProvider _sp;
        private Task _backgroundTask;
        private CancellationTokenSource _cts;
        private bool _isLock = false;

        public UploadJob(IUploadQueue queue, IServiceProvider sp)
        {
            _queue = queue;
            _sp = sp;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            _backgroundTask = Task.Run(async () =>
            {
                while (await timer.WaitForNextTickAsync(_cts.Token))
                {
                    try
                    {
                        await ExecuteAsync();
                    }
                    catch (OperationCanceledException)
                    {
                        // Uygulama kapanırken task iptal ediliyor
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now} - [JOB ERROR] {ex.Message}");
                    }
                }
            }, _cts.Token);

            return Task.CompletedTask;
        }

        private async Task ExecuteAsync()
        {
            if (_isLock) return;
            _isLock = true;

            Console.WriteLine($"{DateTime.Now} --JOB STARTED--");
            Console.WriteLine($"{DateTime.Now} --Queue item count: {_queue.Count}");

            //var items = _queue.DequeueBatch(50); // batch size -> memory kontrol

            var jsonItems = await _queue.DequeueBatchAsync("photoQueue", 50);

            if (jsonItems.Count > 0)
            {
                var items = jsonItems
                                .Select(i => JsonConvert.DeserializeObject<CachedUpload>(i))
                                .Where(i => i != null)
                                .ToList();//JsonConvert.DeserializeObject<List<CachedUpload>>(jsonItems);

                if (items is null || items.Count == 0)
                {
                    _isLock = false;
                    return;
                }

                Console.WriteLine($"{DateTime.Now} Processing {items.Count} items...");

                using var scope = _sp.CreateScope();
                var cloudinary = scope.ServiceProvider.GetRequiredService<CloudinaryService>();
                var photoService = scope.ServiceProvider.GetRequiredService<IPhotoRepository>();

                foreach (var item in items)
                {
                    try
                    {
                        // Base64 -> byte array
                        //byte[] bytes = Convert.FromBase64String(item.Base64);

                        byte[] bytes = item.FileBytes;

                        //if (item.FileBytes != null)
                        //{
                        //    bytes = item.FileBytes;
                        //}

                        // MemoryStream kullanımı (yazılabilir=false)
                        //await using var ms = new MemoryStream(bytes, writable: false);

                        await using var ms = new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: true);


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

                        // Belleği serbest bırak
                        bytes = null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now} - [JOB ERROR] File {item.FileName} - {ex}");
                    }
                }

                await photoService.SaveAsync();

            }
            Console.WriteLine($"{DateTime.Now} --JOB COMPLETED SUCCESSFULLY--");
            _isLock = false;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            return Task.CompletedTask;
        }
    }
}
