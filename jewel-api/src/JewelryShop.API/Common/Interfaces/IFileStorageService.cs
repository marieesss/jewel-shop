namespace JewelryShop.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Sauvegarde un fichier et renvoie son URL/chemin public relatif.
    /// </summary>
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string subFolder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un fichier précédemment stocké (no-op si introuvable).
    /// </summary>
    void Delete(string? publicPath);
}
