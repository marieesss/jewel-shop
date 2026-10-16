using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public record UploadCharmImageCommand(
    long CharmId,
    Stream Content,
    string FileName,
    string ContentType) : IRequest<string>;
