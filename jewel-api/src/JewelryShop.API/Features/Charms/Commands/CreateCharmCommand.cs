using FluentValidation;
using JewelryShop.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public record CreateCharmCommand(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    int Stock,
    string? Url = null) : IRequest<CharmDto>;

public class CreateCharmCommandValidator : AbstractValidator<CreateCharmCommand>
{
    public CreateCharmCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom est requis.")
            .MaximumLength(255).WithMessage("255 caractères maximum.");
        RuleFor(x => x.Description)
            .MaximumLength(10000).WithMessage("10000 caractères maximum.");
        RuleFor(x => x.Color)
            .IsInEnum().WithMessage("Couleur invalide.");
        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Le coût doit être positif.");
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix doit être positif.");
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Le stock doit être positif.");
        RuleFor(x => x.Url)
            .MaximumLength(500).WithMessage("500 caractères maximum.");
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
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Color = request.Color,
            Cost = request.Cost,
            Price = request.Price,
            Stock = request.Stock,
            Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim()
        };

        await _charms.AddAsync(charm, cancellationToken);
        await _charms.SaveChangesAsync(cancellationToken);

        return charm.ToDto();
    }
}
