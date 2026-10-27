using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Users.Common;
using JewelryShop.Features.Users.DTOs;
using MediatR;

namespace JewelryShop.Features.Users.Queries;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ProfileDto>
{
    private readonly IUserRepository _users;

    public GetMyProfileQueryHandler(IUserRepository users) => _users = users;

    public async Task<ProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsNoTrackingAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(UserMessages.EntityName, request.UserId);

        return user.ToProfileDto();
    }
}
