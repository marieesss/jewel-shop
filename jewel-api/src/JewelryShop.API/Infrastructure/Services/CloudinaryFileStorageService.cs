using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using JewelryShop.Application.Common.Interfaces;
using System.Text.RegularExpressions;

namespace JewelryShop.Infrastructure.Services;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary__CloudName manquant dans .env");
        var apiKey    = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary__ApiKey manquant dans .env");
        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary__ApiSecret manquant dans .env");

        var account  = new Account(cloudName, apiKey, apiSecret);
        _cloudinary  = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File   = new FileDescription(fileName, content),
            Folder = $"jewelryshop/{subFolder}",

            // f_auto : Cloudinary choisit le format optimal pour chaque navigateur
            //          (WebP pour Chrome, AVIF si supporté, JPEG sinon…)
            // q_auto : qualité optimisée automatiquement — réduit la taille du fichier
            //          sans perte visible à l'œil nu
            EagerTransforms = new List<Transformation>
            {
                new Transformation().FetchFormat("auto").Quality("auto")
            },
            EagerAsync = false
        };

        var result = await Task.Run(() => _cloudinary.Upload(uploadParams), cancellationToken);

        if (result.Error is not null)
            throw new InvalidOperationException($"Cloudinary upload error: {result.Error.Message}");

        // On préfère l'URL eager (déjà optimisée f_auto + q_auto)
        // Fallback sur SecureUrl brute si l'eager n'est pas disponible
        var optimizedUrl = result.Eager?.FirstOrDefault()?.SecureUrl?.ToString()
                        ?? result.SecureUrl.ToString();

        return optimizedUrl;
    }

    public void Delete(string? publicPath)
    {
        if (string.IsNullOrWhiteSpace(publicPath)) return;

        // Extrait le public_id Cloudinary depuis l'URL stockée.
        // Format attendu : .../upload/[transformations/][v123/]jewelryshop/{subFolder}/{id}.ext
        // Le public_id est la partie "jewelryshop/{subFolder}/{id}" (sans extension).
        var match = Regex.Match(publicPath, @"jewelryshop/(.+?)(?:\.[a-z0-9]+)?$",
            RegexOptions.IgnoreCase);

        if (!match.Success) return;

        var publicId = "jewelryshop/" + match.Groups[1].Value;
        _cloudinary.Destroy(new DeletionParams(publicId));
    }
}
