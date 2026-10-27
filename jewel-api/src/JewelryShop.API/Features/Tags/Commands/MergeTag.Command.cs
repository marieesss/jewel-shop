using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Commands;

/// <summary>Fusionne le tag <paramref name="Id"/> dans <paramref name="TargetTagId"/> : ses articles passent sur le tag cible, puis il est supprimé.</summary>
public record MergeTagCommand(long Id, long TargetTagId) : IRequest<TagDto>;
