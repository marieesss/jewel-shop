using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Auth;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Le_hash_ne_contient_pas_le_mot_de_passe_en_clair()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.DoesNotContain("Secret123!", hash);
    }

    [Fact]
    public void Verify_accepte_le_bon_mot_de_passe_et_refuse_un_autre()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.True(_hasher.Verify("Secret123!", hash));
        Assert.False(_hasher.Verify("Mauvais123!", hash));
    }
}
