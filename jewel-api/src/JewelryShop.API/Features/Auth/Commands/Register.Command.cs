using JewelryShop.Features.Auth.DTOs;
using MediatR;

namespace JewelryShop.Features.Auth.Commands;

/// <summary>
/// Le compte prend l'email de l'invitation désignée par <paramref name="InvitationToken"/>, obligatoire.
/// <paramref name="Email"/> est ignoré : il reste accepté pour ne pas casser les clients qui l'envoient encore.
/// <paramref name="InvitationToken"/> reste nullable pour qu'un token absent donne une erreur de validation
/// et non une erreur de désérialisation.
/// </summary>
public record RegisterCommand(
    string Name,
    string Surname,
    string? Email,
    string Password,
    DateOnly? Birthday,
    string? InvitationToken = null) : IRequest<RegisterResult>;

public record RegisterResult(AuthResponse Response, string RefreshToken) : AuthResult(Response, RefreshToken);
