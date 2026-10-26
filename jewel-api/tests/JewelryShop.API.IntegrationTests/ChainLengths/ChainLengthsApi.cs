using System.Net.Http.Json;
using JewelryShop.Features.ChainLengths.DTOs;

namespace JewelryShop.API.IntegrationTests.ChainLengths;

public static class ChainLengthsApi
{
    public const string PublicPath = "/api/chain-lengths";
    public const string AdminPath = "/api/admin/chain-lengths";

    public static async Task<List<ChainLengthDto>> GetChainLengthsAsync(this HttpClient client, string path = PublicPath)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<ChainLengthDto>>())!;
    }
}
