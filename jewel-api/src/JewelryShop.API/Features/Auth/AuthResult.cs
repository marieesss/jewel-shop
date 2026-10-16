using JewelryShop.Application.Features.Auth.DTOs;

namespace JewelryShop.Application.Features.Auth;

/// <summary>
/// Résultat interne d'une authentification. Le controller pose <see cref="RefreshToken"/> en cookie
/// et ne renvoie que <see cref="Response"/> : le refresh token n'apparaît jamais dans un corps de réponse.
/// </summary>
public record AuthResult(AuthResponse Response, string RefreshToken);
