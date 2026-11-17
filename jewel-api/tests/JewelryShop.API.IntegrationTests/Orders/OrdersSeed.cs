using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Orders;

/// <summary>Commandes insérées ou modifiées directement en base.</summary>
public static class OrdersSeed
{
    public const string ProductInOrder = "Ce produit figure dans une commande.";

    /// <summary>Commande d'un nouveau client, avec une seule ligne choisie par le test.</summary>
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

    /// <summary>Commande sans ligne d'un utilisateur donné, dans le statut et à la date choisis. Renvoie son id.</summary>
    public static async Task<long> SeedOrderForAsync(
        this JewelryShopApiFactory api, long userId, OrderStatus status, DateTime createdAt)
    {
        var order = new Order
        {
            Number = $"CMD-T-{Guid.NewGuid():N}"[..OrderRules.NumberMaxLength],
            UserId = userId,
            Status = status,
            RecipientName = "Martin",
            RecipientSurname = "Alice",
            RecipientPhone = "0600000000",
            AddressLine1 = "1 rue des Lilas",
            PostalCode = "75001",
            City = "Paris",
            Total = 10m,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
        await api.SeedAsync(async services =>
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        });
        return order.Id;
    }

    /// <summary>Change le statut d'une commande en base, comme le ferait un admin.</summary>
    public static Task SetOrderStatusAsync(this JewelryShopApiFactory api, long orderId, OrderStatus status)
        => api.SeedAsync(services => services.GetRequiredService<AppDbContext>().Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.Status, status)));

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
