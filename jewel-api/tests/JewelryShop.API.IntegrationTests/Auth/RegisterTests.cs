using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Invitations.Common;
using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Auth;

[Collection(ApiCollection.Name)]
public class RegisterTests
{
    private readonly JewelryShopApiFactory _api;
    private readonly HttpClient _client;

    public RegisterTests(JewelryShopApiFactory api)
    {
        _api = api;
        _client = api.CreateApiClient();
    }

    [Fact]
    public async Task Cree_le_compte_ouvre_la_session_et_pose_le_cookie_du_refresh_token()
    {
        var (_, token) = await _api.CreatePendingInvitationAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/register", RegisterBody(token));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await SessionAssertions.AssertSessionOpenedAsync(response);
    }

    [Fact]
    public async Task Cree_un_compte_qui_peut_se_connecter_avec_l_email_de_l_invitation()
    {
        var (email, token) = await _api.CreatePendingInvitationAsync();
        await _client.PostAsJsonAsync("/api/auth/register", RegisterBody(token));

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = TestAccounts.Password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Passe_l_invitation_a_acceptee()
    {
        var (email, token) = await _api.CreatePendingInvitationAsync();

        await _client.PostAsJsonAsync("/api/auth/register", RegisterBody(token));

        var status = await _api.QueryAsync(services => services.GetRequiredService<AppDbContext>().Invitations
            .Where(i => i.Email == email)
            .Select(i => i.Status)
            .SingleAsync());
        Assert.Equal(InvitationStatus.Accepted, status);
    }

    [Fact]
    public async Task Refuse_un_token_d_invitation_inconnu_avec_400_sans_poser_de_cookie()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", RegisterBody("token-inconnu"));

        await AssertInvalidInvitationAsync(response);
    }

    [Fact]
    public async Task Refuse_une_invitation_deja_acceptee_avec_400()
    {
        var (_, token) = await _api.CreatePendingInvitationAsync();
        await _client.PostAsJsonAsync("/api/auth/register", RegisterBody(token));

        var response = await _client.PostAsJsonAsync("/api/auth/register", RegisterBody(token));

        await AssertInvalidInvitationAsync(response);
    }

    private static object RegisterBody(string invitationToken) => new
    {
        Name = "Marie",
        Surname = "Test",
        Password = TestAccounts.Password,
        Birthday = (DateOnly?)null,
        InvitationToken = invitationToken
    };

    private static async Task AssertInvalidInvitationAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        var messages = Assert.Contains("InvitationToken", error.Errors);
        Assert.Contains(InvitationDefaults.InvalidInvitationMessage, messages);
        Assert.Null(RefreshTokenCookies.Find(response));
    }
}
