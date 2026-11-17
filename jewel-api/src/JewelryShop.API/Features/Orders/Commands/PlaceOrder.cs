using FluentValidation.Results;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Users.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace JewelryShop.Features.Orders.Commands;

/// <summary>
/// Crée une commande <c>Pending</c> et prélève son stock dans la même transaction. Le stock est retiré par des
/// UPDATE conditionnels : une commande simultanée qui a pris le dernier stock fait échouer celle-ci en 409,
/// et tout est annulé, commande comprise. La confirmation au client et l'avis aux admins sont mis en file
/// dans la même transaction (outbox) : un serveur de mail indisponible ne fait pas échouer la commande.
/// </summary>
public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly NecklaceComposer _composer;
    private readonly IEmailQueue _emailQueue;
    private readonly AppOptions _appOptions;
    private readonly TimeProvider _timeProvider;

    public PlaceOrderCommandHandler(
        IOrderRepository orders,
        IUserRepository users,
        NecklaceComposer composer,
        IEmailQueue emailQueue,
        IOptions<AppOptions> appOptions,
        TimeProvider timeProvider)
    {
        _orders = orders;
        _users = users;
        _composer = composer;
        _emailQueue = emailQueue;
        _appOptions = appOptions.Value;
        _timeProvider = timeProvider;
    }

    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsNoTrackingAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(UserMessages.EntityName, request.UserId);

        var recipient = ResolveRecipient(request.Recipient, user);
        var address = ResolveAddress(request.Address, user);
        var necklace = await _composer.ComposeAsync(request, cancellationToken);
        var admins = await _users.GetByRoleAsNoTrackingAsync(UserRole.Admin, cancellationToken);

        Order order = null!;
        await _orders.InTransactionAsync(async () =>
        {
            var rollId = await _orders.TakeChainLengthAsync(necklace.Chain.Id, necklace.LengthCm, cancellationToken)
                ?? throw new ConflictException(NecklaceMessages.OutOfStock(necklace.Chain.Name));

            await TakeCharmsAsync(necklace.Charms, cancellationToken);

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var sequenceValue = await _orders.NextNumberSequenceValueAsync(cancellationToken);
            order = BuildOrder(request, necklace, rollId, recipient, address, OrderNumber.Format(now, sequenceValue), now);
            await _orders.AddAsync(order, cancellationToken);

            if (request.SaveAddressToProfile && request.Address is not null)
                await SaveAddressToProfileAsync(request.UserId, address, cancellationToken);

            // Les liens des mails portent l'id de la commande, attribué par la base au premier enregistrement.
            await _orders.SaveChangesAsync(cancellationToken);
            OrderCreatedEmails.Enqueue(_emailQueue, _appOptions, order, user, admins);
            await _orders.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return order.ToOrderDto();
    }

    private static OrderRecipientDto ResolveRecipient(OrderRecipientInput? input, User user)
    {
        var phone = string.IsNullOrWhiteSpace(input?.Phone) ? user.Phone : input.Phone.Trim();
        if (string.IsNullOrWhiteSpace(phone))
            throw new ValidationException([new ValidationFailure($"{nameof(PlaceOrderCommand.Recipient)}.{nameof(OrderRecipientInput.Phone)}", UserMessages.PhoneRequired)]);

        return input is null
            ? new OrderRecipientDto(user.Name, user.Surname, phone)
            : new OrderRecipientDto(input.Name.Trim(), input.Surname.Trim(), phone);
    }

    private static OrderAddressDto ResolveAddress(OrderAddressInput? input, User user)
    {
        if (input is not null)
        {
            return new OrderAddressDto(
                input.Line1.Trim(),
                string.IsNullOrWhiteSpace(input.Line2) ? null : input.Line2.Trim(),
                input.PostalCode,
                input.City.Trim());
        }

        if (!user.HasAddress)
            throw new ValidationException([new ValidationFailure(nameof(PlaceOrderCommand.Address), OrderMessages.AddressRequired)]);

        return new OrderAddressDto(user.AddressLine1!, user.AddressLine2, user.PostalCode!, user.City!);
    }

    // Par ordre d'id, pour que deux commandes qui partagent des breloques verrouillent leurs lignes
    // dans le même ordre et ne s'attendent pas mutuellement.
    private async Task TakeCharmsAsync(IReadOnlyList<Charm> charms, CancellationToken cancellationToken)
    {
        foreach (var copies in charms.GroupBy(c => c.Id).OrderBy(g => g.Key))
        {
            if (!await _orders.TakeCharmStockAsync(copies.Key, copies.Count(), cancellationToken))
                throw new ConflictException(NecklaceMessages.OutOfStock(copies.First().Name));
        }
    }

    private async Task SaveAddressToProfileAsync(long userId, OrderAddressDto address, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(UserMessages.EntityName, userId);

        user.AddressLine1 = address.Line1;
        user.AddressLine2 = address.Line2;
        user.PostalCode = address.PostalCode;
        user.City = address.City;
        _users.Update(user);
    }

    private static Order BuildOrder(
        PlaceOrderCommand request,
        Necklace necklace,
        long rollId,
        OrderRecipientDto recipient,
        OrderAddressDto address,
        string number,
        DateTime now)
    {
        var chain = necklace.Chain;
        var chainLine = new OrderLine
        {
            Kind = OrderLineKind.Chain,
            ChainId = chain.Id,
            ChainRollId = rollId,
            Name = chain.Name,
            LengthCm = necklace.LengthCm,
            PricePerMeter = chain.PricePerMeter,
            ClaspPrice = chain.ClaspPrice,
            UnitPrice = necklace.ChainPrice
        };

        var charmLines = necklace.Charms.Select((charm, index) => new OrderLine
        {
            Kind = OrderLineKind.Charm,
            CharmId = charm.Id,
            Name = charm.Name,
            UnitPrice = charm.Price,
            Position = NecklaceRules.FirstPosition + index
        });

        return new Order
        {
            Number = number,
            UserId = request.UserId,
            Status = OrderStatus.Pending,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            RecipientName = recipient.Name,
            RecipientSurname = recipient.Surname,
            RecipientPhone = recipient.Phone,
            AddressLine1 = address.Line1,
            AddressLine2 = address.Line2,
            PostalCode = address.PostalCode,
            City = address.City,
            Total = necklace.Total,
            CreatedAt = now,
            UpdatedAt = now,
            Lines = [chainLine, .. charmLines],
            StatusHistory =
            [
                new OrderStatusHistory
                {
                    FromStatus = null,
                    ToStatus = OrderStatus.Pending,
                    ChangedBy = request.UserId,
                    CustomerNotified = true,
                    CreatedAt = now
                }
            ]
        };
    }
}
