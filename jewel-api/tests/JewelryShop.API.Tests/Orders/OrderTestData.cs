using JewelryShop.Features.Orders.Entities;

namespace JewelryShop.API.Tests.Orders;

/// <summary>Commande complète, avec une chaîne prise dans un rouleau et deux breloques, pour les tests de suivi.</summary>
public static class OrderTestData
{
    public const long ChainId = 3;
    public const long RollId = 100;
    public const int LengthCm = 45;
    public const long CloverId = 10;
    public const long HeartId = 11;

    public static Order NewOrder(long id, long userId, OrderStatus status, DateTime createdAt) => new()
    {
        Id = id,
        Number = $"CMD-2026-{id:0000}",
        UserId = userId,
        Status = status,
        RecipientName = "Marie",
        RecipientSurname = "Durand",
        RecipientPhone = "0612345678",
        AddressLine1 = "12 rue des Lilas",
        PostalCode = "75011",
        City = "Paris",
        Total = 16m,
        CreatedAt = createdAt,
        UpdatedAt = createdAt,
        Lines =
        [
            new OrderLine { Kind = OrderLineKind.Charm, CharmId = CloverId, Name = "Trèfle", UnitPrice = 5m, Position = 2 },
            new OrderLine
            {
                Kind = OrderLineKind.Chain,
                ChainId = ChainId,
                ChainRollId = RollId,
                Name = "Forçat",
                LengthCm = LengthCm,
                UnitPrice = 6.5m
            },
            new OrderLine { Kind = OrderLineKind.Charm, CharmId = HeartId, Name = "Cœur", UnitPrice = 4.5m, Position = 1 }
        ],
        StatusHistory =
        [
            new OrderStatusHistory { Id = id * 10, FromStatus = null, ToStatus = OrderStatus.Pending, ChangedBy = userId, CreatedAt = createdAt }
        ]
    };
}
