using JewelryShop.Features.Auth.DTOs;
using MediatR;

namespace JewelryShop.Features.Auth.Commands;

/// <summary>
/// Avec <paramref name="InvitationToken"/>, le compte prend l'email de l'invitation et <paramref name="Email"/>
/// est ignoré : il peut être absent ou vide.
/// </summary>
public record RegisterCommand(
    string Name,
    string Surname,
    string? Email,
    string Password,
    DateOnly? Birthday,
    string? InvitationToken = null) : IRequest<RegisterResult>;

public record RegisterResult(AuthResponse Response, string RefreshToken) : AuthResult(Response, RefreshToken);
