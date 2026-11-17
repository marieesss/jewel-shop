using JewelryShop.Common.Interfaces;
using JewelryShop.Infrastructure.Email;

namespace JewelryShop.Features.Orders.Common;

/// <summary>Avis aux admins après une action du client sur sa commande (annulation, réception).</summary>
public static class OrderCustomerActionEmails
{
    /// <summary>Met en file un mail par admin avec le template donné, sans sauvegarder.</summary>
    public static void Enqueue(
        IEmailQueue emailQueue,
        AppOptions appOptions,
        string template,
        Order order,
        User customer,
        IEnumerable<User> admins)
    {
        var model = new
        {
            CustomerName = $"{customer.Name} {customer.Surname}",
            CustomerEmail = customer.Email,
            Order = OrderEmailSummary.From(order),
            OrderUrl = appOptions.FrontendUrl($"/admin/orders/{order.Id}")
        };
        foreach (var admin in admins)
            emailQueue.Enqueue(admin.Email, template, model);
    }
}
