using JewelryShop.Common.Interfaces;
using JewelryShop.Infrastructure.Email;

namespace JewelryShop.Features.Orders.Common;

/// <summary>Mails d'une nouvelle commande : la confirmation au client et un avis à chaque admin.</summary>
public static class OrderCreatedEmails
{
    /// <summary>Met les mails en file, sans sauvegarder. La commande doit déjà avoir son id, utilisé dans les liens.</summary>
    public static void Enqueue(
        IEmailQueue emailQueue, AppOptions appOptions, Order order, User customer, IEnumerable<User> admins)
    {
        var summary = OrderEmailSummary.From(order);

        emailQueue.Enqueue(customer.Email, EmailTemplates.OrderConfirmation, new
        {
            FirstName = customer.Name,
            Order = summary,
            OrderUrl = appOptions.FrontendUrl($"/orders/{order.Id}")
        });

        var adminModel = new
        {
            CustomerName = $"{customer.Name} {customer.Surname}",
            CustomerEmail = customer.Email,
            Order = summary,
            OrderUrl = appOptions.FrontendUrl($"/admin/orders/{order.Id}")
        };
        foreach (var admin in admins)
            emailQueue.Enqueue(admin.Email, EmailTemplates.OrderCreatedAdmin, adminModel);
    }
}
