using JewelryShop.Features.Options.DTOs;
using MediatR;

namespace JewelryShop.Features.Options.Queries;

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
