using System.Net.Http.Json;
using JewelryShop.Features.Users.DTOs;

namespace JewelryShop.API.IntegrationTests.Users;

public static class UsersApi
{
    public const string MePath = "/api/users/me";
    public const string MyAddressPath = "/api/users/me/address";

    public static async Task<ProfileDto> GetMyProfileAsync(this HttpClient client)
    {
        var response = await client.GetAsync(MePath);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProfileDto>())!;
    }
}
