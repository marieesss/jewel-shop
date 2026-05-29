using FluentValidation;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Charms.Commands;

public record UpdateCharmCommand(
    long Id,
    string Name,
    string Description,
    ProductColor Color,
    string? Url,
    decimal Cost,
    decimal Price,
    int Stock) : IRequest<CharmDto>;

public class UpdateCharmCommandValidator : AbstractValidator<UpdateCharmCommand>
{
    public UpdateCharmCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.Color).IsInEnum();
        RuleFor(x => x.Url).MaximumLength(500);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCharmCommandHandler : IRequestHandler<UpdateCharmCommand, CharmDto>
{
    private readonly ICharmRepository _charms;

    public UpdateCharmCommandHandler(ICharmRepository charms) => _charms = charms;

    public async Task<CharmDto> Handle(UpdateCharmCommand request, CancellationToken cancellationToken)
    {
        var charm = await _charms.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Breloque", request.Id);

        charm.Name = request.Name.Trim();
        charm.Description = request.Description?.Trim() ?? string.Empty;
        charm.Color = request.Color;
        charm.Url = request.Url?.Trim();
        charm.Cost = request.Cost;
        charm.Price = request.Price;
        charm.Stock = request.Stock;

        _charms.Update(charm);
        await _charms.SaveChangesAsync(cancellationToken);

        return charm.ToDto();
    }
}
