namespace JewelryShop.Application.Features.Auth.DTOs;

public record AuthResponse(string Token, long UserId, string Role);
