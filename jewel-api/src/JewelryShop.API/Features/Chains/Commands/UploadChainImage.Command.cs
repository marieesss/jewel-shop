using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public record UploadChainImageCommand(
    long ChainId,
    Stream Content,
    long Length,
    string FileName,
    string ContentType) : IRequest<string>;
