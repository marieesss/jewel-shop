using JewelryShop.Application.Features.Options.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Options.Queries;

public record GetOptionsQuery : IRequest<IEnumerable<OptionDto>>;

public class GetOptionsQueryHandler : IRequestHandler<GetOptionsQuery, IEnumerable<OptionDto>>
{
    private readonly IOptionRepository _options;

    public GetOptionsQueryHandler(IOptionRepository options) => _options = options;

    public async Task<IEnumerable<OptionDto>> Handle(GetOptionsQuery request, CancellationToken cancellationToken)
    {
        var options = await _options.GetAllAsync(cancellationToken);
        return options
            .OrderBy(o => o.Length)
            .Select(o => o.ToDto())
            .ToList();
    }
}
