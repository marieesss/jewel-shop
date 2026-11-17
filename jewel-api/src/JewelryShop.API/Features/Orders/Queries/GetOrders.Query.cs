using JewelryShop.Common.Models;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

/// <summary>Commandes de tous les clients, des plus récentes aux plus anciennes. Sans statut, tous les statuts.</summary>
public record GetOrdersQuery(
    string? Status,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize) : IRequest<PagedResult<AdminOrderSummaryDto>>;
