using FluentValidation;
using JewelryShop.Application.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Charms.Commands;

public record CreateCharmCommand(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    int Stock) : IRequest<CharmDto>;

public class CreateCharmCommandValidator : AbstractValidator<CreateCharmCommand>
{
    public CreateCharmCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.Color).IsInEnum();
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}

public class CreateCharmCommandHandler : IRequestHandler<CreateCharmCommand, CharmDto>
{
    private readonly ICharmRepository _charms;

    public CreateCharmCommandHandler(ICharmRepository charms) => _charms = charms;

    public async Task<CharmDto> Handle(CreateCharmCommand request, CancellationToken cancellationToken)
    {
        var charm = new Charm
        {
            Name        = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Color       = request.Color,
            Cost        = request.Cost,
            Price       = request.Price,
            Stock       = request.Stock
        };

        await _charms.AddAsync(charm, cancellationToken);
        await _charms.SaveChangesAsync(cancellationToken);

        return charm.ToDto();
    }
}
