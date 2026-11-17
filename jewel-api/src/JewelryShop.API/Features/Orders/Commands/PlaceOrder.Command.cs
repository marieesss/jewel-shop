using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

/// <summary>
/// Commande d'un collier. Sans <see cref="Recipient"/>, le destinataire est le titulaire du compte ;
/// sans <see cref="Address"/>, l'adresse du profil est utilisée.
/// </summary>
public record PlaceOrderCommand(
    long UserId,
    long ChainId,
    int LengthCm,
    IReadOnlyList<long> CharmIds,
    string? Comment,
    OrderRecipientInput? Recipient,
    OrderAddressInput? Address,
    bool SaveAddressToProfile) : IRequest<OrderDto>, INecklaceComposition;

/// <summary>Destinataire saisi pour une autre personne. Sans téléphone, celui du compte est utilisé.</summary>
public record OrderRecipientInput(string Name, string Surname, string? Phone);

public record OrderAddressInput(string Line1, string? Line2, string PostalCode, string City);
