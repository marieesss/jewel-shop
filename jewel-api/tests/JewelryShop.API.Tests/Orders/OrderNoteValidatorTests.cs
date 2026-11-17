using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Validators;

namespace JewelryShop.API.Tests.Orders;

public class OrderNoteValidatorTests
{
    private readonly AddOrderNoteCommandValidator _addValidator = new();
    private readonly UpdateOrderNoteCommandValidator _updateValidator = new();

    [Fact]
    public void Accepte_une_note_de_2000_caracteres()
    {
        var content = new string('a', 2000);

        Assert.True(_addValidator.Validate(new AddOrderNoteCommand(1, 1, content)).IsValid);
        Assert.True(_updateValidator.Validate(new UpdateOrderNoteCommand(1, 1, content)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Refuse_une_note_vide(string content)
    {
        ValidationAssert.HasError(
            _addValidator.Validate(new AddOrderNoteCommand(1, 1, content)),
            nameof(AddOrderNoteCommand.Content),
            "La note est requise.");
        ValidationAssert.HasError(
            _updateValidator.Validate(new UpdateOrderNoteCommand(1, 1, content)),
            nameof(UpdateOrderNoteCommand.Content),
            "La note est requise.");
    }

    [Fact]
    public void Refuse_une_note_de_plus_de_2000_caracteres()
    {
        var content = new string('a', 2001);

        ValidationAssert.HasError(
            _addValidator.Validate(new AddOrderNoteCommand(1, 1, content)),
            nameof(AddOrderNoteCommand.Content),
            "2000 caractères maximum.");
        ValidationAssert.HasError(
            _updateValidator.Validate(new UpdateOrderNoteCommand(1, 1, content)),
            nameof(UpdateOrderNoteCommand.Content),
            "2000 caractères maximum.");
    }
}
