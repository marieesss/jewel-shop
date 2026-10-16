using JewelryShop.Common.Interfaces;

namespace JewelryShop.Infrastructure.Email;

public class EmailQueue : IEmailQueue
{
    private readonly IEmailTemplateRenderer _renderer;
    private readonly IEmailOutboxRepository _repository;
    private readonly TimeProvider _timeProvider;

    public EmailQueue(IEmailTemplateRenderer renderer, IEmailOutboxRepository repository, TimeProvider timeProvider)
    {
        _renderer = renderer;
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public void Enqueue(string to, string templateName, object model)
    {
        var email = _renderer.Render(templateName, model);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        _repository.Add(EmailOutboxMessage.Create(to, email, now));
    }
}
