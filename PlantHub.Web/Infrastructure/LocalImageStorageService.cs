using Microsoft.AspNetCore.Components.Forms;

namespace PlantHub.Web.Infrastructure;

public interface IImageStorageService
{
    Task<string> SavePlantImageAsync(IBrowserFile file);
    void DeleteImage(string imageUrl);
}

public class LocalImageStorageService : IImageStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalImageStorageService> _logger;
    private const long MaxSize = 5 * 1024 * 1024;

    public LocalImageStorageService(IWebHostEnvironment env, ILogger<LocalImageStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> SavePlantImageAsync(IBrowserFile file)
    {
        var ext = Path.GetExtension(file.Name).ToLowerInvariant();

        if (ext is not (".jpg" or ".jpeg" or ".png"))
            throw new InvalidOperationException("Only JPG and PNG are supported");

        var fileName = $"{Guid.NewGuid()}{ext}";

        // URL to store in DB
        var url = $"/local/plant-hub/plants/{fileName}";

        // Physical file path (local environment)
        var root = Path.Combine(_env.WebRootPath, "local", "plant-hub", "plants");
        Directory.CreateDirectory(root);

        var physicalPath = Path.Combine(root, fileName);

        await using var fs = File.Create(physicalPath);
        await file.OpenReadStream(MaxSize).CopyToAsync(fs);

        _logger.LogInformation("Saved plant image at: {Path}", physicalPath);
        return url;
    }

    public void DeleteImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        var relative = imageUrl.TrimStart('/'); // local/plant-hub/...
        var physical = Path.Combine(_env.WebRootPath, relative);

        if (File.Exists(physical))
        {
            File.Delete(physical);
            _logger.LogInformation("Deleted plant image: {Path}", physical);
        }
    }
}
