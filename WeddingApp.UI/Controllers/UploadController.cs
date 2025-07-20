using Microsoft.AspNetCore.Mvc;
using Wedding.Repository.Interfaces;

namespace WeddingApp.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly IPhotoRepository _photoService;

        public UploadController(CloudinaryService cloudinaryService, IPhotoRepository photoService)
        {
            _cloudinaryService = cloudinaryService;
            _photoService = photoService;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(file);
            if (imageUrl == null)
                return BadRequest("Yükleme başarısız.");

            _photoService.Add(new Wedding.Model.DB.PhotoDb { PublicId = imageUrl, Extension = file.FileName.Split('.')[1] == "HEIC" ? "jpg" : file.FileName.Split('.')[1] });

            await _photoService.SaveAsync();

            return Ok(new { url = imageUrl });
        }

        [HttpGet("photos")]
        public async Task<IActionResult> GetPhotos([FromServices] CloudinaryService cloudinaryService)
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
