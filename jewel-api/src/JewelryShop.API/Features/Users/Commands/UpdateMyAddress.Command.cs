using JewelryShop.Features.Users.DTOs;
using MediatR;

namespace JewelryShop.Features.Users.Commands;

public record UpdateMyAddressCommand(
    long UserId,
    string Line1,
    string? Line2,
    string PostalCode,
    string City,
    string Phone) : IRequest<ProfileDto>;
