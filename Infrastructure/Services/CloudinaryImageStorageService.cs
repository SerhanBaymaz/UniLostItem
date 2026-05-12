using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AppImageUploadResult = Application.Interfaces.ImageUploadResult;

namespace Infrastructure.Services;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryImageStorageService> _logger;

    public CloudinaryImageStorageService(IConfiguration configuration, ILogger<CloudinaryImageStorageService> logger)
    {
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary:CloudName yapılandırılmamış");
        var apiKey = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary:ApiKey yapılandırılmamış");
        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary:ApiSecret yapılandırılmamış");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<AppImageUploadResult> UploadImageAsync(
        Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "unilostitem",
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error != null)
        {
            _logger.LogError("Cloudinary yükleme hatası: {Error}", uploadResult.Error.Message);
            throw new InvalidOperationException($"Görsel yüklenemedi: {uploadResult.Error.Message}");
        }

        return new AppImageUploadResult
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        };
    }

    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Result == "ok")
        {
            return true;
        }

        _logger.LogWarning("Cloudinary silme hatası - PublicId: {PublicId}, Sonuç: {Result}", publicId, result.Result);
        return false;
    }
}
