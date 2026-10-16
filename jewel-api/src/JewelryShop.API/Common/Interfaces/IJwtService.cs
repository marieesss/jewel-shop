
namespace JewelryShop.Application.Common.Interfaces;

public interface IJwtService
{
    /// <summary>
    /// Génère un JWT HS256 (claims sub/email/role) pour l'utilisateur donné.
    /// </summary>
    string GenerateToken(User user);
}
