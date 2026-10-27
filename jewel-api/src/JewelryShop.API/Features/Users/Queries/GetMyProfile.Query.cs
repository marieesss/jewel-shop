using JewelryShop.Features.Users.DTOs;
using MediatR;

namespace JewelryShop.Features.Users.Queries;

public record GetMyProfileQuery(long UserId) : IRequest<ProfileDto>;
