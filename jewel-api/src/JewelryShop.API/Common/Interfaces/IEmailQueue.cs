namespace JewelryShop.Common.Interfaces;

public interface IEmailQueue
{
    /// <summary>
    /// Rend le template et ajoute le mail au contexte EF, sans appeler SaveChanges :
    /// le mail est enregistré par le SaveChangesAsync de l'action métier, dans la même transaction.
    /// </summary>
    void Enqueue(string to, string templateName, object model);
}
