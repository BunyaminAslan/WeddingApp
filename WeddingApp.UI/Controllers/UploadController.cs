using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Wedding.Repository.Interfaces;
using WeddingApp.UI.Cache;
using WeddingApp.UI.Models;
using WeddingApp.UI.Redis;

namespace WeddingApp.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly IPhotoRepository _photoService;
        private readonly FallbackQueueService _uploadCache;

        private readonly IRedisQueueService _redisQueue;
        public UploadController(CloudinaryService cloudinaryService, IPhotoRepository photoService, FallbackQueueService uploadCache, IRedisQueueService redisQueue)
        {
            _cloudinaryService = cloudinaryService;
            _photoService = photoService;
            _uploadCache = uploadCache;
            _redisQueue = redisQueue;
        }


        [RequestSizeLimit(600_000_000)] // 600MB
        [HttpPost("multi")]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files)
        {
            Log.Logger.Information($"UploadImages started. Files Count: {files.Count}");

            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var cacheItem = new CachedUpload
                {
                    FileName = file.FileName,
                    FileBytes = ms.ToArray(), // Base64 yok
                    ContentType = file.ContentType,
                    ReceivedAt = DateTime.UtcNow,
                    Ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "1",
                    Device = Request.Headers["User-Agent"].ToString()
                };

                #region MemoryCache
                // Queue içine direkt byte[] olarak at


                //_uploadCache.Enqueue(cacheItem); 
                #endregion

                
                await _uploadCache.EnqueueAsync("photoQueue", JsonConvert.SerializeObject(cacheItem));

            }

            Log.Logger.Information($"UploadImages done. Queue Count: {_uploadCache.Count}");

            return Ok(new { message = "Fotoğraflar sıraya alındı, birazdan yüklenecek." });
        }
        

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(file);
            if (imageUrl == null)
                return BadRequest("Yükleme başarısız.");

            var extension = Path.GetExtension(file.FileName)?.TrimStart('.').ToLowerInvariant() ?? "jpg";
            if (extension == "heic")
                extension = "jpg";

            var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
              ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            var userAgent = Request.Headers["User-Agent"].ToString();


            await _photoService.AddAsync(new Wedding.Model.DB.PhotoDb { PublicId = imageUrl, Extension = extension, Ip = ipAddress, Device = userAgent });

            await _photoService.SaveAsync();

            return Ok(new { url = imageUrl });
        }

        [HttpGet("photos")]
        public async Task<IActionResult> GetPhotos([FromServices] CloudinaryService cloudinaryService)
        {
            try
            {
                /*var photos = (await _photoService.GetAllAsync())
                    .OrderByDescending(p => p.created_at)
                    .Select(p => new {
                        url = cloudinaryService.GetAuthenticatedUrl(p.PublicId, p.Extension)
                    })
                    .ToList().Take(10);
                
                return Ok(photos);*/
                return Ok(new CloudinaryService());
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Galeri listelenirken bir hata oluştu",
                    error = ex.Message
                });
            }
        }


        [HttpGet("private/photos")]
        public async Task<IActionResult> PhotosAsync([FromServices] CloudinaryService cloudinaryService)
        {
            try
            {
                var photos = (await _photoService.GetAllAsync())
                    .OrderByDescending(p => p.created_at)
                    .Select(p => new
                    {
                        url = cloudinaryService.GetAuthenticatedUrl(p.PublicId, p.Extension)
                    })
                    .ToList();

                return Ok(photos);

            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Galeri listelenirken bir hata oluştu",
                    error = ex.Message
                });
            }
        }

        //Slider Photo

        [HttpPost("slider/image")]
        public async Task<IActionResult> SliderUploadImage([FromForm] IFormFile file)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(file);
            if (imageUrl == null)
                return BadRequest("Yükleme başarısız.");

            _photoService.Add(new Wedding.Model.DB.PhotoDb { PublicId = imageUrl, Extension = file.FileName.Split('.')[1] == "HEIC" ? "jpg" : file.FileName.Split('.')[1] });

            await _photoService.SaveAsync();

            return Ok(new { url = imageUrl });
        }

        [HttpGet("slider/photos")]
        public async Task<IActionResult> SliderGetPhotos([FromServices] CloudinaryService cloudinaryService)
        {
            try
            {
                var photos = (await _photoService.GetAllAsync())
                    .OrderByDescending(p => p.created_at)
                    .Select(p => new {
                        url = cloudinaryService.GetAuthenticatedUrl(p.PublicId, p.Extension)
                    })
                    .ToList();

                return Ok(photos);
            }
            catch (Exception ex)
            {
                // Logla (dilersen burada log servisi varsa kullan)
                return BadRequest(new
                {
                    message = "Galeri listelenirken bir hata oluştu",
                    error = ex.Message
                });
            }
        }

    }

}
