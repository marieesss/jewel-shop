using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.ChainLengths.DTOs;

namespace JewelryShop.API.IntegrationTests.ChainLengths;

// La base est partagée entre les tests : chaque test utilise ses propres longueurs.
[Collection(ApiCollection.Name)]
public class AdminChainLengthsTests
{
    private const string AlreadyExists = "Cette longueur existe déjà.";

    private readonly JewelryShopApiFactory _api;

    public AdminChainLengthsTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Ajoute_une_longueur_qui_apparait_triee_dans_la_liste_publique()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync(ChainLengthsApi.AdminPath, new { LengthCm = 48, Label = "Princesse longue" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ChainLengthDto>();
        Assert.NotNull(created);
        Assert.Equal(48, created.LengthCm);
        Assert.Equal("Princesse longue", created.Label);

        var lengths = (await _api.CreateApiClient().GetChainLengthsAsync()).Select(l => l.LengthCm).ToList();
        Assert.Equal(lengths.IndexOf(45) + 1, lengths.IndexOf(48));
        Assert.Equal(lengths.IndexOf(48) + 1, lengths.IndexOf(50));
    }

    [Fact]
    public async Task Refuse_une_longueur_deja_existante_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync(ChainLengthsApi.AdminPath, new { LengthCm = 45, Label = "Doublon" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(AlreadyExists, error?.Title);
    }

    [Fact]
    public async Task Refuse_une_longueur_hors_bornes_avec_400()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync(ChainLengthsApi.AdminPath, new { LengthCm = 5, Label = "Trop court" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains("La longueur doit être comprise entre 10 et 200 cm.", Assert.Contains("LengthCm", error.Errors));
    }

    [Fact]
    public async Task Refuse_l_ajout_a_un_utilisateur_non_admin_avec_403()
    {
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(ChainLengthsApi.AdminPath, new { LengthCm = 150, Label = "Interdit" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Refuse_la_liste_admin_sans_connexion_avec_401()
    {
        var response = await _api.CreateApiClient().GetAsync(ChainLengthsApi.AdminPath);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Renvoie_la_liste_admin_triee()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var lengths = await admin.GetChainLengthsAsync(ChainLengthsApi.AdminPath);

        Assert.Contains(lengths, l => l.LengthCm == 38 && l.Label == "Ras du cou");
        Assert.Equal(lengths.Select(l => l.LengthCm).Order(), lengths.Select(l => l.LengthCm));
    }

    [Fact]
    public async Task Modifie_une_longueur()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var created = await CreateAsync(admin, 151, "A modifier");

        var response = await admin.PutAsJsonAsync($"{ChainLengthsApi.AdminPath}/{created.Id}", new { LengthCm = 152, Label = "Modifiée" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ChainLengthDto>();
        Assert.Equal(new ChainLengthDto(created.Id, 152, "Modifiée"), updated);
        var lengths = await _api.CreateApiClient().GetChainLengthsAsync();
        Assert.DoesNotContain(lengths, l => l.LengthCm == 151);
    }

    [Fact]
    public async Task Refuse_de_modifier_vers_une_longueur_deja_prise_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var created = await CreateAsync(admin, 153, "A modifier");

        var response = await admin.PutAsJsonAsync($"{ChainLengthsApi.AdminPath}/{created.Id}", new { LengthCm = 38, Label = "Doublon" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(AlreadyExists, error?.Title);
    }

    [Fact]
    public async Task Refuse_de_modifier_une_longueur_inconnue_avec_404()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PutAsJsonAsync($"{ChainLengthsApi.AdminPath}/999999", new { LengthCm = 154, Label = "Inconnue" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Supprime_une_longueur()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var created = await CreateAsync(admin, 155, "A supprimer");

        var response = await admin.DeleteAsync($"{ChainLengthsApi.AdminPath}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var lengths = await _api.CreateApiClient().GetChainLengthsAsync();
        Assert.DoesNotContain(lengths, l => l.Id == created.Id);
    }

    [Fact]
    public async Task Refuse_de_supprimer_une_longueur_inconnue_avec_404()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.DeleteAsync($"{ChainLengthsApi.AdminPath}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<ChainLengthDto> CreateAsync(HttpClient admin, int lengthCm, string label)
    {
        var response = await admin.PostAsJsonAsync(ChainLengthsApi.AdminPath, new { LengthCm = lengthCm, Label = label });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ChainLengthDto>())!;
    }
}
