namespace FulSpectrum.Api.Services;

public sealed class LocalImageStorageService : IImageStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    public LocalImageStorageService(
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct)
    {
        var ext = Path.GetExtension(fileName);
        var safeExt = string.IsNullOrWhiteSpace(ext) ? ".bin" : ext.ToLowerInvariant();
        var uploadsRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var generated = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{safeExt}";
        var fullPath = Path.Combine(uploadsRoot, generated);

        await using var fs = File.Create(fullPath);
        await stream.CopyToAsync(fs, ct);
        var relativePath = $"/uploads/{generated}";
        var cdnBaseUrl = _configuration["Images:CdnBaseUrl"];
        if (!string.IsNullOrWhiteSpace(cdnBaseUrl))
        {
            return $"{cdnBaseUrl.TrimEnd('/')}{relativePath}";
        }



        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return relativePath;
        }

        return $"{request.Scheme}://{request.Host}{relativePath}";
    }
}
