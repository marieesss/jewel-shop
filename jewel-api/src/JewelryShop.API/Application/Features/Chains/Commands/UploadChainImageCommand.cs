using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Common.Interfaces;
using MediatR;

namespace JewelryShop.Application.Features.Chains.Commands;

public record UploadChainImageCommand(
    long ChainId,
    Stream Content,
    string FileName,
    string ContentType) : IRequest<string>;

public class UploadChainImageCommandHandler : IRequestHandler<UploadChainImageCommand, string>
{
    private readonly IChainRepository _chains;
    private readonly IFileStorageService _storage;

    public UploadChainImageCommandHandler(IChainRepository chains, IFileStorageService storage)
    {
        _chains = chains;
        _storage = storage;
    }

    public async Task<string> Handle(UploadChainImageCommand request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdAsync(request.ChainId, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.ChainId);

        var newUrl = await _storage.SaveAsync(request.Content, request.FileName, "chains", cancellationToken);

        // Supprime l'ancienne image si elle existait
        if (!string.IsNullOrWhiteSpace(chain.ImageUrl))
            _storage.Delete(chain.ImageUrl);

        chain.ImageUrl = newUrl;
        _chains.Update(chain);
        await _chains.SaveChangesAsync(cancellationToken);

        return newUrl;
    }
}
