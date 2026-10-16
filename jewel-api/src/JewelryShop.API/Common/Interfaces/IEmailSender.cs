using JewelryShop.Application.Common.Models;

namespace JewelryShop.Application.Common.Interfaces;

public interface IEmailSender
{
    /// <summary>
    /// Lève une exception si l'envoi échoue : c'est l'outbox (#25) qui gère les nouvelles tentatives.
    /// </summary>
    Task SendAsync(EmailMessage message, CancellationToken ct);
}
