using JewelryShop.Application.Common.Models;

namespace JewelryShop.Application.Common.Interfaces;

public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Les propriétés du modèle sont exposées aux templates en snake_case (FirstName → first_name).
    /// Lève une InvalidOperationException si le template est introuvable, invalide,
    /// ou utilise une variable absente du modèle.
    /// </summary>
    RenderedEmail Render(string templateName, object model);
}
