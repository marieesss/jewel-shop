using JewelryShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace JewelryShop.Infrastructure.Services;

/// <summary>
/// Stocke les fichiers dans wwwroot/uploads/{subFolder}/ et retourne le chemin relatif.
/// Swappable vers S3 en remplaçant uniquement cette classe.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        var ext       = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var folder    = Path.Combine(_webRootPath, "uploads", subFolder);

        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, uniqueName);
        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fs, cancellationToken);

        // Retourne le chemin public relatif (servi par StaticFiles)
        return $"/uploads/{subFolder}/{uniqueName}";
    }

    public void Delete(string? publicPath)
    {
        if (string.IsNullOrWhiteSpace(publicPath)) return;

        // Convertit "/uploads/chains/xxx.jpg" → chemin absolu
        var relative = publicPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_webRootPath, relative);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
