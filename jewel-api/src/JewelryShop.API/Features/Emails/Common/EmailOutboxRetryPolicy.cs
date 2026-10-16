namespace JewelryShop.Features.Emails.Common;

public static class EmailOutboxRetryPolicy
{
    /// <summary>Délais avant les tentatives 2 à 6, après un premier envoi en échec.</summary>
    public static readonly IReadOnlyList<TimeSpan> RetryDelays = new[]
    {
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(60),
        TimeSpan.FromMinutes(240)
    };

    public static int MaxAttempts => RetryDelays.Count + 1;

    public const int MaxErrorLength = 2000;
}
