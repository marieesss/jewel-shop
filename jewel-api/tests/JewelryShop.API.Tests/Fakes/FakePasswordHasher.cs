using JewelryShop.Application.Common.Interfaces;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>Hache par préfixe : le vrai BCrypt est testé à part et ralentirait les tests.</summary>
public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hash:{password}";

    public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
}
