namespace JewelryShop.Features.Orders.DTOs;

/// <summary>
/// Détail d'une commande pour l'admin : les champs de <see cref="OrderDto"/>, le client, l'historique complet
/// des statuts et les notes internes.
/// </summary>
public record AdminOrderDto(
    long Id,
    string Number,
    string Status,
    string? Comment,
    OrderRecipientDto Recipient,
    OrderAddressDto Address,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total,
    IReadOnlyList<AdminOrderStatusChangeDto> StatusHistory,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    AdminOrderCustomerDto Customer,
    IReadOnlyList<OrderNoteDto> Notes);

public record AdminOrderCustomerDto(long Id, string Name, string Surname, string Email);

/// <summary>Changement de statut, avec son auteur et ce qui a été dit au client.</summary>
public record AdminOrderStatusChangeDto(
    string? FromStatus,
    string ToStatus,
    OrderUserDto Author,
    string? Message,
    bool CustomerNotified,
    DateTime CreatedAt);

public static class AdminOrderMappingExtensions
{
    /// <summary>
    /// <paramref name="users"/> contient le client et tous les auteurs de l'historique et des notes.
    /// </summary>
    public static AdminOrderDto ToAdminOrderDto(this Order order, IReadOnlyDictionary<long, User> users)
    {
        var dto = order.ToOrderDto();
        var customer = users[order.UserId];

        return new AdminOrderDto(
            dto.Id,
            dto.Number,
            dto.Status,
            dto.Comment,
            dto.Recipient,
            dto.Address,
            dto.Lines,
            dto.Total,
            order.StatusHistory
                .OrderBy(h => h.CreatedAt)
                .ThenBy(h => h.Id)
                .Select(h => new AdminOrderStatusChangeDto(
                    h.FromStatus?.ToString(),
                    h.ToStatus.ToString(),
                    users[h.ChangedBy].ToOrderUserDto(),
                    h.Message,
                    h.CustomerNotified,
                    h.CreatedAt))
                .ToList(),
            dto.CreatedAt,
            dto.UpdatedAt,
            new AdminOrderCustomerDto(customer.Id, customer.Name, customer.Surname, customer.Email),
            order.Notes
                .OrderBy(n => n.CreatedAt)
                .ThenBy(n => n.Id)
                .Select(n => n.ToOrderNoteDto(users[n.AuthorId]))
                .ToList());
    }

    /// <summary>Comptes à charger pour <see cref="ToAdminOrderDto"/>.</summary>
    public static IReadOnlyCollection<long> RelatedUserIds(this Order order) =>
        order.StatusHistory.Select(h => h.ChangedBy)
            .Concat(order.Notes.Select(n => n.AuthorId))
            .Append(order.UserId)
            .ToHashSet();
}
