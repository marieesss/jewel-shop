using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Invitations.DTOs;
using MediatR;

namespace JewelryShop.Features.Invitations.Queries;

public class GetInvitationsQueryHandler : IRequestHandler<GetInvitationsQuery, PagedResult<InvitationDto>>
{
    private readonly IInvitationRepository _invitations;
    private readonly TimeProvider _timeProvider;

    public GetInvitationsQueryHandler(IInvitationRepository invitations, TimeProvider timeProvider)
    {
        _invitations = invitations;
        _timeProvider = timeProvider;
    }

    public async Task<PagedResult<InvitationDto>> Handle(GetInvitationsQuery request, CancellationToken cancellationToken)
    {
        // Le validateur a déjà refusé un statut inconnu.
        InvitationQueries.TryParseStatus(request.Status, out var status);
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var (items, total) = await _invitations.GetPagedAsync(status, now, page, pageSize, cancellationToken);

        var dtos = items.Select(i => i.ToDto(now)).ToList();
        return new PagedResult<InvitationDto>(dtos, total, page, pageSize);
    }
}
