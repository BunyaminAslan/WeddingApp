using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using WeddingApp.UI.ImageUpload;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService()
    {
        var account = new Account(
            Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
            Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
            Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
        );

        Console.WriteLine(account.Cloud);
        Console.WriteLine(account.ApiSecret);
        Console.WriteLine(account.ApiKey);


        _cloudinary = new Cloudinary(account);
    }

    public Cloudinary GetClient() => _cloudinary;

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        try
        {
            if (file.Length == 0)
                return null;

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "wedding/private/images",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                Type = "authenticated" //  Bu satır önemli!
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.PublicId?.ToString();
        }
        catch (Exception ex)
        {

            throw new Exception("UploadImageAsync Error : " + ex.Message);
        }
    }

    public string GetAuthenticatedUrl(string publicId, string extension = "jpg")
    {
        try
        {
            var fullPublicId = $"{publicId}.{extension}";

            var url = _cloudinary.Api.UrlImgUp
                .Type("authenticated")
                .Secure(true)
                .Signed(true)
                .Version(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
                //.PublicId(publicId)
                .BuildUrl($"{publicId}.{extension}");

            return url;
        }
        catch (Exception ex)
        {
            throw new Exception(message : "GetAuthenticatedUrl hatası " + ex.Message);
        }
    }
    /*
     public string GetSignedUrl(string publicId, string extension, int expirationInSeconds = 120)
    {
        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expiry = unixTimestamp + expirationInSeconds;

        var parameters = new SortedDictionary<string, object>
    {
        { "public_id", $"wedding/private/images/{publicId}" },
        { "timestamp", expiry },
        { "type", "authenticated" }
    };

        var signature = _cloudinary.Api.SignParameters(parameters);

        var url = $"https://res.cloudinary.com/{_cloudinary.Api.Account.Cloud}/image/authenticated/" +
                  $"wedding/private/images/{publicId}.{extension}" +
                  $"?timestamp={expiry}&signature={signature}&api_key={_cloudinary.Api.Account.ApiKey}";

        return url;
    }
    */
    //public string GetSignedImageUrl(string publicId)
    //{
    //    return _cloudinary.Api.UrlImgUp
    //        .Secure(true)
    //        .Type("authenticated")
    //        .Signed(true)
    //        .Exp(DateTime.UtcNow.AddMinutes(30))
    //        .BuildUrl($"{publicId}.jpg"); // .png olabilir, uzantıyı doğru sakla
    //}
}
