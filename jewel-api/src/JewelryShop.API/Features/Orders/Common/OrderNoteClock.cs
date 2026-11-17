namespace JewelryShop.Features.Orders.Common;

public static class OrderNoteClock
{
    /// <summary>
    /// Instant présent arrondi à la microseconde, la précision des colonnes timestamp de PostgreSQL : la note
    /// renvoyée après l'ajout ou la modification a ainsi les mêmes dates que celle relue ensuite.
    /// </summary>
    public static DateTime UtcNow(TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMicrosecond));
    }
}
