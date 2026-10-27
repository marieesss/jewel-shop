using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Users.Common;
using JewelryShop.Features.Users.DTOs;
using MediatR;

namespace JewelryShop.Features.Users.Commands;

public class UpdateMyAddressCommandHandler : IRequestHandler<UpdateMyAddressCommand, ProfileDto>
{
    private readonly IUserRepository _users;

    public UpdateMyAddressCommandHandler(IUserRepository users) => _users = users;

    public async Task<ProfileDto> Handle(UpdateMyAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(UserMessages.EntityName, request.UserId);

        user.AddressLine1 = request.Line1.Trim();
        user.AddressLine2 = string.IsNullOrWhiteSpace(request.Line2) ? null : request.Line2.Trim();
        user.PostalCode = request.PostalCode;
        user.City = request.City.Trim();
        user.Phone = request.Phone;

        _users.Update(user);
        await _users.SaveChangesAsync(cancellationToken);

        return user.ToProfileDto();
    }
}
