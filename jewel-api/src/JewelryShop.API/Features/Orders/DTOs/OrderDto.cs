using JewelryShop.Features.Orders.Common;

namespace JewelryShop.Features.Orders.DTOs;

/// <summary>
/// Détail d'une commande. Le destinataire, l'adresse, les noms et les prix sont ceux copiés à la commande :
/// ils ne suivent pas le profil ni le catalogue.
/// </summary>
public record OrderDto(
    long Id,
    string Number,
    string Status,
    string? Comment,
    OrderRecipientDto Recipient,
    OrderAddressDto Address,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total,
    IReadOnlyList<OrderStatusChangeDto> StatusHistory,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record OrderRecipientDto(string Name, string Surname, string Phone);

public record OrderAddressDto(string Line1, string? Line2, string PostalCode, string City);

/// <summary>
/// Ligne de la commande : la chaîne porte sa longueur et ses prix au mètre et du fermoir,
/// chaque breloque sa position (à partir de 1).
/// </summary>
public record OrderLineDto(
    string Kind,
    long ProductId,
    string Name,
    int? LengthCm,
    int? Position,
    decimal? PricePerMeter,
    decimal? ClaspPrice,
    decimal UnitPrice);

/// <summary>Changement de statut. <see cref="FromStatus"/> est null pour la création de la commande.</summary>
public record OrderStatusChangeDto(string? FromStatus, string ToStatus, DateTime CreatedAt);

public static class OrderMappingExtensions
{
    public static OrderDto ToOrderDto(this Order order) => new(
        order.Id,
        order.Number,
        order.Status.ToString(),
        order.Comment,
        new OrderRecipientDto(order.RecipientName, order.RecipientSurname, order.RecipientPhone),
        new OrderAddressDto(order.AddressLine1, order.AddressLine2, order.PostalCode, order.City),
        order.Lines
            .OrderBy(l => l.Kind == OrderLineKind.Chain ? 0 : 1)
            .ThenBy(l => l.Position ?? NecklaceRules.FirstPosition)
            .Select(ToOrderLineDto)
            .ToList(),
        order.Total,
        order.StatusHistory
            .OrderBy(h => h.CreatedAt)
            .ThenBy(h => h.Id)
            .Select(h => new OrderStatusChangeDto(h.FromStatus?.ToString(), h.ToStatus.ToString(), h.CreatedAt))
            .ToList(),
        order.CreatedAt,
        order.UpdatedAt);

    private static OrderLineDto ToOrderLineDto(OrderLine line) => new(
        line.Kind.ToString(),
        line.ChainId ?? line.CharmId!.Value,
        line.Name,
        line.LengthCm,
        line.Position,
        line.PricePerMeter,
        line.ClaspPrice,
        line.UnitPrice);
}
