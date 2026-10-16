using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common;
using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Entities;
using JewelryShop.Features.Emails.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Email;
using JewelryShop.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace JewelryShop.API.Tests.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeUserRepository _users = new();
    private readonly FakePasswordResetTokenRepository _resetTokens = new();
    private readonly FakeEmailOutboxRepository _outbox = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        var timeProvider = new FakeTimeProvider(_now);
        var emailQueue = new EmailQueue(new ScribanEmailTemplateRenderer(), _outbox, timeProvider);
        _handler = new ForgotPasswordCommandHandler(
            _users, _resetTokens, _tokenGenerator, emailQueue, Options.Create(new AppOptions()), timeProvider);
        _users.Seed(new User { Id = 1, Name = "Marie", Email = "marie@mycharmly.eu" });
    }

    [Fact]
    public async Task Cree_un_token_hache_valable_1_heure_pour_un_email_inscrit()
    {
        await _handler.Handle(new ForgotPasswordCommand(" Marie@MyCharmly.eu "), CancellationToken.None);

        var token = Assert.Single(_resetTokens.All);
        Assert.Equal(1, token.UserId);
        Assert.Equal(64, token.TokenHash.Length);
        Assert.Equal(_now.UtcDateTime.AddHours(1), token.ExpiresAt);
    }

    [Fact]
    public async Task Met_en_file_le_mail_avec_le_lien_du_token()
    {
        await _handler.Handle(new ForgotPasswordCommand("marie@mycharmly.eu"), CancellationToken.None);

        var email = Assert.Single(_outbox.All);
        Assert.Equal("marie@mycharmly.eu", email.ToAddress);
        Assert.Equal(EmailOutboxStatus.Pending, email.Status);
        Assert.Contains("Bonjour Marie,", email.TextBody);
        var token = ExtractToken(email.TextBody);
        Assert.Equal(_resetTokens.All.Single().TokenHash, _tokenGenerator.Hash(token));
    }

    [Fact]
    public async Task Enregistre_le_token_et_le_mail_dans_une_seule_sauvegarde()
    {
        await _handler.Handle(new ForgotPasswordCommand("marie@mycharmly.eu"), CancellationToken.None);

        Assert.Equal(1, _users.SaveChangesCount);
        Assert.Equal(0, _resetTokens.SaveChangesCount);
        Assert.Equal(0, _outbox.SaveChangesCount);
    }

    [Fact]
    public async Task Ne_cree_ni_token_ni_mail_pour_un_email_inconnu()
    {
        await _handler.Handle(new ForgotPasswordCommand("inconnu@mycharmly.eu"), CancellationToken.None);

        Assert.Empty(_resetTokens.All);
        Assert.Empty(_outbox.All);
        Assert.Equal(0, _users.SaveChangesCount);
    }

    [Fact]
    public async Task Une_nouvelle_demande_supprime_les_tokens_non_utilises_de_la_precedente()
    {
        await _handler.Handle(new ForgotPasswordCommand("marie@mycharmly.eu"), CancellationToken.None);
        var first = _resetTokens.All.Single();

        await _handler.Handle(new ForgotPasswordCommand("marie@mycharmly.eu"), CancellationToken.None);

        var second = Assert.Single(_resetTokens.All);
        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task Garde_les_tokens_deja_utilises()
    {
        var used = PasswordResetToken.Create(_users.All.Single(), new string('a', 64), _now.UtcDateTime);
        used.MarkUsed(_now.UtcDateTime);
        _resetTokens.Seed(used);

        await _handler.Handle(new ForgotPasswordCommand("marie@mycharmly.eu"), CancellationToken.None);

        Assert.Contains(used, _resetTokens.All);
        Assert.Equal(2, _resetTokens.All.Count);
    }

    private static string ExtractToken(string body)
    {
        const string prefix = "http://localhost:5173/reset-password?token=";
        var start = body.IndexOf(prefix, StringComparison.Ordinal);
        Assert.True(start >= 0, "Le lien de réinitialisation est absent du mail.");
        return new string(body[(start + prefix.Length)..].TakeWhile(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
