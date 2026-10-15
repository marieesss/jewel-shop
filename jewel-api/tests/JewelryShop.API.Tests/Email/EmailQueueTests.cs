using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Features.Emails;
using JewelryShop.Infrastructure.Email;

namespace JewelryShop.API.Tests.Email;

public class EmailQueueTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeEmailOutboxRepository _repository = new();
    private readonly EmailQueue _queue;

    public EmailQueueTests()
    {
        _queue = new EmailQueue(new ScribanEmailTemplateRenderer(), _repository, new FakeTimeProvider(_now));
    }

    [Fact]
    public void Ajoute_le_mail_rendu_en_attente()
    {
        _queue.Enqueue("cliente@example.com", EmailTemplates.Test, new { FirstName = "Marie" });

        var message = Assert.Single(_repository.All);
        Assert.Equal("cliente@example.com", message.ToAddress);
        Assert.Equal("Bonjour Marie", message.Subject.Trim());
        Assert.Contains("Bonjour Marie", message.TextBody);
        Assert.Equal(EmailOutboxStatus.Pending, message.Status);
        Assert.Equal(_now.UtcDateTime, message.NextAttemptAt);
    }

    [Fact]
    public void Ne_sauvegarde_pas_le_mail_lui_meme()
    {
        _queue.Enqueue("cliente@example.com", EmailTemplates.Test, new { FirstName = "Marie" });

        Assert.Equal(0, _repository.SaveChangesCount);
    }

    [Fact]
    public void N_ajoute_rien_quand_le_template_est_introuvable()
    {
        Assert.Throws<InvalidOperationException>(
            () => _queue.Enqueue("cliente@example.com", "Inexistant", new { FirstName = "Marie" }));

        Assert.Empty(_repository.All);
    }
}
