using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Users.Common;
using Microsoft.Extensions.Options;

namespace JewelryShop.Features.Orders.Common;

/// <summary>
/// Changement de statut demandé par le client sur sa propre commande (annulation, réception). Le statut change
/// par un UPDATE conditionnel sur le statut de départ : de deux actions simultanées (un double clic, ou le client
/// et un admin), une seule passe et l'autre reçoit un 409, ce qui évite de recréditer le stock deux fois.
/// L'historique, la remise en stock et l'avis aux admins (outbox) sont dans la même transaction.
/// </summary>
public class CustomerOrderTransition
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly IEmailQueue _emailQueue;
    private readonly AppOptions _appOptions;
    private readonly TimeProvider _timeProvider;

    public CustomerOrderTransition(
        IOrderRepository orders,
        IUserRepository users,
        IEmailQueue emailQueue,
        IOptions<AppOptions> appOptions,
        TimeProvider timeProvider)
    {
        _orders = orders;
        _users = users;
        _emailQueue = emailQueue;
        _appOptions = appOptions.Value;
        _timeProvider = timeProvider;
    }

    /// <param name="refusedMessage">Message du 409 quand la commande n'est pas dans un statut qui le permet.</param>
    /// <param name="adminTemplate">Template de l'avis envoyé à chaque admin.</param>
    public async Task<OrderDto> ApplyAsync(
        long userId,
        long orderId,
        OrderStatus to,
        string refusedMessage,
        string adminTemplate,
        CancellationToken cancellationToken)
    {
        var current = await _orders.GetCustomerOrderAsNoTrackingAsync(userId, orderId, cancellationToken);
        var from = current.Status;
        if (!OrderStateMachine.CanTransition(from, to, OrderActor.Customer))
            throw new ConflictException(refusedMessage);

        var customer = await _users.GetByIdAsNoTrackingAsync(userId, cancellationToken)
            ?? throw new NotFoundException(UserMessages.EntityName, userId);
        var admins = await _users.GetByRoleAsNoTrackingAsync(UserRole.Admin, cancellationToken);

        Order order = null!;
        await _orders.InTransactionAsync(async () =>
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (!await _orders.TryChangeStatusAsync(orderId, userId, from, to, now, cancellationToken))
                throw new ConflictException(refusedMessage);

            // Rechargée après l'UPDATE : elle porte déjà le nouveau statut et la date de modification.
            order = await _orders.GetByIdWithDetailsAsync(orderId, cancellationToken)
                ?? throw new NotFoundException(OrderMessages.NotFound);

            if (to == OrderStatus.Cancelled && OrderStateMachine.RestocksOnCancellation(from))
                await OrderStock.ReturnAsync(_orders, order, cancellationToken);

            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                FromStatus = from,
                ToStatus = to,
                ChangedBy = userId,
                CustomerNotified = false,
                CreatedAt = now
            });

            OrderCustomerActionEmails.Enqueue(_emailQueue, _appOptions, adminTemplate, order, customer, admins);
            await _orders.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return order.ToOrderDto();
    }
}
