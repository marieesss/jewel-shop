namespace JewelryShop.Features.Orders.DTOs;

/// <summary>Commande dans la liste admin des commandes, avec le compte client qui l'a passée.</summary>
public record AdminOrderSummaryDto(
    long Id,
    string Number,
    DateTime CreatedAt,
    string Status,
    decimal Total,
    OrderUserDto Customer);

/// <summary>Compte lié à une commande : son client, l'auteur d'un changement de statut ou d'une note.</summary>
public record OrderUserDto(long Id, string Name, string Surname);

public static class AdminOrderSummaryMappingExtensions
{
    public static AdminOrderSummaryDto ToAdminOrderSummaryDto(this Order order, User customer) =>
        new(order.Id, order.Number, order.CreatedAt, order.Status.ToString(), order.Total, customer.ToOrderUserDto());

    public static OrderUserDto ToOrderUserDto(this User user) => new(user.Id, user.Name, user.Surname);
}
