using Microsoft.AspNetCore.Components.Forms;

namespace PlantHub.Web.Infrastructure;

public class AddonImageStorageService : IImageStorageService
{
    private readonly ILogger<AddonImageStorageService> _logger;
    private const long MaxSize = 5 * 1024 * 1024;

    // Hard-coded HA media folder
    private const string BasePath = "/config/www/plant-hub/plants";

    public AddonImageStorageService(ILogger<AddonImageStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SavePlantImageAsync(IBrowserFile file)
    {
        var ext = Path.GetExtension(file.Name).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png"))
            throw new InvalidOperationException("Only JPG and PNG are supported");

        Directory.CreateDirectory(BasePath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var physicalPath = Path.Combine(BasePath, fileName);

        await using var fs = File.Create(physicalPath);
        await file.OpenReadStream(MaxSize).CopyToAsync(fs);

        _logger.LogInformation("Saved plant image in HA storage: {Path}", physicalPath);

        // same URL as local version
        return $"/local/plant-hub/plants/{fileName}";
    }

    public void DeleteImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        var fileName = imageUrl.Split('/').LastOrDefault();
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        var physical = Path.Combine(BasePath, fileName);

        if (File.Exists(physical))
        {
            File.Delete(physical);
            _logger.LogInformation("Deleted HA plant image: {Path}", physical);
        }
    }
}