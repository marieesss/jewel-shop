using JewelryShop.Features.Orders.Common;

namespace JewelryShop.API.Tests.Orders;

public class OrderNumberTests
{
    [Theory]
    [InlineData(1, "CMD-2026-0001")]
    [InlineData(42, "CMD-2026-0042")]
    [InlineData(9999, "CMD-2026-9999")]
    public void Complete_le_compteur_sur_4_chiffres(long sequenceValue, string expected)
    {
        Assert.Equal(expected, OrderNumber.Format(new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc), sequenceValue));
    }

    [Fact]
    public void Garde_tous_les_chiffres_au_dela_de_9999()
    {
        Assert.Equal("CMD-2026-12345", OrderNumber.Format(new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc), 12345));
    }

    [Fact]
    public void Continue_le_compteur_d_une_annee_sur_l_autre()
    {
        var lastOf2026 = OrderNumber.Format(new DateTime(2026, 12, 31, 23, 59, 0, DateTimeKind.Utc), 42);
        var firstOf2027 = OrderNumber.Format(new DateTime(2027, 1, 1, 0, 1, 0, DateTimeKind.Utc), 43);

        Assert.Equal("CMD-2026-0042", lastOf2026);
        Assert.Equal("CMD-2027-0043", firstOf2027);
    }
}
