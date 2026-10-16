using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Common.Interfaces;
using MediatR;

namespace JewelryShop.Application.Features.Charms.Commands;

public record UploadCharmImageCommand(
    long CharmId,
    Stream Content,
    string FileName,
    string ContentType) : IRequest<string>;

public class UploadCharmImageCommandHandler : IRequestHandler<UploadCharmImageCommand, string>
{
    private readonly ICharmRepository _charms;
    private readonly IFileStorageService _storage;

    public UploadCharmImageCommandHandler(ICharmRepository charms, IFileStorageService storage)
    {
        _charms = charms;
        _storage = storage;
    }

    public async Task<string> Handle(UploadCharmImageCommand request, CancellationToken cancellationToken)
    {
        var charm = await _charms.GetByIdAsync(request.CharmId, cancellationToken)
            ?? throw new NotFoundException("Breloque", request.CharmId);

        var newUrl = await _storage.SaveAsync(request.Content, request.FileName, "charms", cancellationToken);

        if (!string.IsNullOrWhiteSpace(charm.ImageUrl))
            _storage.Delete(charm.ImageUrl);

        charm.ImageUrl = newUrl;
        _charms.Update(charm);
        await _charms.SaveChangesAsync(cancellationToken);

        return newUrl;
    }
}
