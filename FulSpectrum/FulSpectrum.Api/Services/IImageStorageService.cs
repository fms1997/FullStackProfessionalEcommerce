namespace FulSpectrum.Api.Services;

public interface IImageStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct);
}
