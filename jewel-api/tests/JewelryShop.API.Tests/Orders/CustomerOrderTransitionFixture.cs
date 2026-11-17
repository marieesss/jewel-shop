using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Email;
using Microsoft.Extensions.Options;

namespace JewelryShop.API.Tests.Orders;

/// <summary>
/// Dépôts en mémoire pour l'annulation et la réception par le client : un client, un autre client, deux admins,
/// le rouleau et les stocks des breloques de <see cref="OrderTestData"/>.
/// </summary>
public class CustomerOrderTransitionFixture
{
    public const long CustomerId = 4;
    public const long OtherCustomerId = 5;
    public const int RollRemainingCm = 155;
    public const int CloverStock = 3;
    public const int HeartStock = 7;

    public static readonly DateTime OrderedAt = new(2026, 11, 10, 8, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime Now = new(2026, 11, 17, 9, 30, 0, DateTimeKind.Utc);

    public FakeOrderRepository Orders { get; } = new();
    public FakeUserRepository Users { get; } = new();
    public FakeEmailOutboxRepository Outbox { get; } = new();
    public CustomerOrderTransition Transition { get; }

    public CustomerOrderTransitionFixture()
    {
        var timeProvider = new FakeTimeProvider(Now);
        Transition = new CustomerOrderTransition(
            Orders,
            Users,
            new EmailQueue(new ScribanEmailTemplateRenderer(), Outbox, timeProvider),
            Options.Create(new AppOptions()),
            timeProvider);

        Orders.Rolls[OrderTestData.RollId] = (OrderTestData.ChainId, RollRemainingCm);
        Orders.CharmStocks[OrderTestData.CloverId] = CloverStock;
        Orders.CharmStocks[OrderTestData.HeartId] = HeartStock;

        Users.Seed(
            new User { Id = CustomerId, Name = "Marie", Surname = "Durand", Email = "marie@example.com" },
            new User { Id = OtherCustomerId, Name = "Léa", Surname = "Petit", Email = "lea@example.com" },
            new User { Id = 1, Name = "Alice", Surname = "Admin", Email = "admin1@mycharmly.eu", Role = UserRole.Admin },
            new User { Id = 2, Name = "Bruno", Surname = "Admin", Email = "admin2@mycharmly.eu", Role = UserRole.Admin });
    }

    public Order SeedOrder(OrderStatus status, long id = 42, long userId = CustomerId)
    {
        var order = OrderTestData.NewOrder(id, userId, status, OrderedAt);
        Orders.Seed(order);
        return order;
    }
}
