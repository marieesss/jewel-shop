
namespace JewelryShop.Application.Features.Users.DTOs;

public record UserDto(
    long Id,
    string Name,
    string Surname,
    string Email,
    DateOnly? Birthday,
    string Role);

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User u) => new(
        u.Id,
        u.Name,
        u.Surname,
        u.Email,
        u.Birthday,
        u.Role.ToString());
}
