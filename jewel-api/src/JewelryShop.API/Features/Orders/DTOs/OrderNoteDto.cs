namespace JewelryShop.Features.Orders.DTOs;

/// <summary>Note interne d'un admin sur une commande, jamais montrée au client.</summary>
public record OrderNoteDto(long Id, string Content, OrderUserDto Author, DateTime CreatedAt, DateTime UpdatedAt);

public static class OrderNoteMappingExtensions
{
    public static OrderNoteDto ToOrderNoteDto(this OrderNote note, User author) =>
        new(note.Id, note.Content, author.ToOrderUserDto(), note.CreatedAt, note.UpdatedAt);
}
