using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public record UploadCharmImageCommand(
    long CharmId,
    Stream Content,
    long Length,
    string FileName,
    string ContentType) : IRequest<string>;
