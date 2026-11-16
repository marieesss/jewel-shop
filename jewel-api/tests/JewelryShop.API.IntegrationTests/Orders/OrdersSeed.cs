using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Orders;

/// <summary>Aucun endpoint ne crée encore de commande : les tests l'insèrent directement en base.</summary>
public static class OrdersSeed
{
    public const string ProductInOrder = "Ce produit figure dans une commande.";

    public static async Task SeedOrderAsync(this JewelryShopApiFactory api, OrderLine line)
    {
        var customer = await api.CreateUserAsync();
        await api.SeedAsync(async services =>
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Orders.Add(new Order
            {
                Number = $"CMD-T-{Guid.NewGuid():N}"[..OrderRules.NumberMaxLength],
                UserId = customer.Id,
                RecipientName = "Martin",
                RecipientSurname = "Alice",
                RecipientPhone = "0600000000",
                AddressLine1 = "1 rue des Lilas",
                PostalCode = "75001",
                City = "Paris",
                Total = line.UnitPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Lines = { line }
            });
            await context.SaveChangesAsync();
        });
    }

    public static OrderLine CharmLine(long charmId) =>
        new() { Kind = OrderLineKind.Charm, CharmId = charmId, Name = "Breloque", UnitPrice = 4.5m, Position = 1 };

    public static OrderLine ChainLine(long chainId) =>
        new()
        {
            Kind = OrderLineKind.Chain,
            ChainId = chainId,
            Name = "Chaîne",
            LengthCm = 45,
            PricePerMeter = 20m,
            ClaspPrice = 3m,
            UnitPrice = 12m
        };
}
