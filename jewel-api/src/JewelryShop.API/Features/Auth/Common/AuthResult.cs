using JewelryShop.Features.Auth.DTOs;

namespace JewelryShop.Features.Auth.Common;

/// <summary>
/// Résultat interne d'une authentification. Le controller pose <see cref="RefreshToken"/> en cookie
/// et ne renvoie que <see cref="Response"/> : le refresh token n'apparaît jamais dans un corps de réponse.
/// </summary>
public abstract record AuthResult(AuthResponse Response, string RefreshToken);
