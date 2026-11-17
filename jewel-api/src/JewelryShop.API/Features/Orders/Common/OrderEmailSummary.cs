using System.Globalization;

namespace JewelryShop.Features.Orders.Common;

/// <summary>
/// Contenu d'une commande commun aux mails : les prix et la date sont déjà formatés en français,
/// les lignes sont dans l'ordre du collier (la chaîne, puis les breloques par position).
/// </summary>
public record OrderEmailSummary(
    string Number,
    string Date,
    IReadOnlyList<OrderEmailLine> Lines,
    string Total,
    string Recipient,
    string Phone,
    IReadOnlyList<string> AddressLines,
    string? Comment)
{
    private const string DateFormat = "dd/MM/yyyy";

    private static readonly NumberFormatInfo _frenchPrice = new() { NumberDecimalSeparator = "," };

    public static OrderEmailSummary From(Order order) => new(
        order.Number,
        order.CreatedAt.ToString(DateFormat, CultureInfo.InvariantCulture),
        order.Lines
            .OrderBy(l => l.Kind == OrderLineKind.Chain ? 0 : 1)
            .ThenBy(l => l.Position ?? NecklaceRules.FirstPosition)
            .Select(l => new OrderEmailLine(LabelOf(l), FormatPrice(l.UnitPrice)))
            .ToList(),
        FormatPrice(order.Total),
        $"{order.RecipientName} {order.RecipientSurname}",
        order.RecipientPhone,
        AddressLinesOf(order),
        order.Comment);

    public static string FormatPrice(decimal price) => price.ToString("0.00", _frenchPrice) + " €";

    private static string LabelOf(OrderLine line) => line.Kind == OrderLineKind.Chain
        ? $"Chaîne {line.Name}, {line.LengthCm} cm"
        : $"Breloque {line.Position} : {line.Name}";

    private static IReadOnlyList<string> AddressLinesOf(Order order) => new[]
        {
            order.AddressLine1,
            order.AddressLine2,
            $"{order.PostalCode} {order.City}"
        }
        .OfType<string>()
        .ToList();
}

public record OrderEmailLine(string Label, string Price);
