using JewelryShop.Features.Creations.Commands;
using JewelryShop.Features.Creations.Validators;

namespace JewelryShop.API.Tests.Creations;

public class CreateCreationCommandValidatorTests
{
    private readonly CreateCreationCommandValidator _validator = new();

    private static CreateCreationCommand ValidCommand(params CreateCreationItemInput[] items) =>
        new(1, 2, 3, items.Length > 0 ? items : [new CreateCreationItemInput(5, 0)]);

    [Fact]
    public void Accepte_une_creation_valide()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(nameof(CreateCreationCommand.UserId))]
    [InlineData(nameof(CreateCreationCommand.ChainId))]
    [InlineData(nameof(CreateCreationCommand.OptionId))]
    public void Refuse_un_identifiant_nul(string propertyName)
    {
        var command = propertyName switch
        {
            nameof(CreateCreationCommand.UserId) => ValidCommand() with { UserId = 0 },
            nameof(CreateCreationCommand.ChainId) => ValidCommand() with { ChainId = 0 },
            _ => ValidCommand() with { OptionId = 0 },
        };

        var result = _validator.Validate(command);

        ValidationAssert.HasError(result, propertyName, "Identifiant invalide.");
    }

    [Fact]
    public void Refuse_une_breloque_avec_un_identifiant_nul()
    {
        var result = _validator.Validate(ValidCommand(new CreateCreationItemInput(0, 0)));

        ValidationAssert.HasError(result, "Items[0].CharmId", "Identifiant invalide.");
    }

    [Fact]
    public void Refuse_une_position_negative()
    {
        var result = _validator.Validate(ValidCommand(new CreateCreationItemInput(5, -1)));

        ValidationAssert.HasError(result, "Items[0].Position", "La position doit être positive.");
    }

    [Fact]
    public void Refuse_une_creation_sans_breloque()
    {
        var result = _validator.Validate(ValidCommand() with { Items = [] });

        ValidationAssert.HasError(
            result,
            nameof(CreateCreationCommand.Items),
            "Une création doit contenir au moins une breloque.");
    }

    [Fact]
    public void Refuse_deux_breloques_a_la_meme_position()
    {
        var command = ValidCommand(new CreateCreationItemInput(5, 1), new CreateCreationItemInput(6, 1));

        var result = _validator.Validate(command);

        ValidationAssert.HasError(
            result,
            nameof(CreateCreationCommand.Items),
            "Les positions des breloques doivent être uniques.");
    }
}
