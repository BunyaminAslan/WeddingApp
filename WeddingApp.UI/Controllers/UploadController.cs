using Microsoft.AspNetCore.Mvc;
using Wedding.Repository.Interfaces;
using WeddingApp.UI.Cache;

namespace WeddingApp.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly IPhotoRepository _photoService;
        private readonly IUploadQueue _uploadCache;

        public UploadController(CloudinaryService cloudinaryService, IPhotoRepository photoService, IUploadQueue uploadCache)
        {
            _cloudinaryService = cloudinaryService;
            _photoService = photoService;
            _uploadCache = uploadCache;
        }
        public class CachedUpload
        {
            public string FileName { get; set; }
            public string Base64 { get; set; }
            public string ContentType { get; set; }
            public string Ip { get; set; }
            public string Device { get; set; }
            public DateTime ReceivedAt { get; set; }
        }

        [RequestSizeLimit(100_000_000)] // örnek: 100MB
        [HttpPost("multi")]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());

                var cacheItem = new CachedUpload
                {
                    FileName = file.FileName,
                    Base64 = base64,
                    ContentType = file.ContentType,
                    ReceivedAt = DateTime.UtcNow,
                    Ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Device = Request.Headers["User-Agent"].ToString()
                };

                _uploadCache.Enqueue(cacheItem);
            }

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
                    .ToList();
                
                return Ok(photos);
                */
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
