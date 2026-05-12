namespace Application.Interfaces;

public class ImageUploadResult
{
    public required string Url { get; set; }
    public required string PublicId { get; set; }
}

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}
