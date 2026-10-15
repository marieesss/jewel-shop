using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Auth;

public class SecureTokenGeneratorTests
{
    private readonly SecureTokenGenerator _generator = new();

    [Fact]
    public void Genere_un_token_de_43_caracteres_en_base64url_sans_padding()
    {
        var token = _generator.Generate();

        Assert.Matches("^[A-Za-z0-9_-]{43}$", token);
    }

    [Fact]
    public void Genere_un_token_different_a_chaque_appel()
    {
        Assert.NotEqual(_generator.Generate(), _generator.Generate());
    }

    [Fact]
    public void Hache_le_token_en_sha256_hexadecimal_minuscule()
    {
        var hash = _generator.Hash("abc");

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }

    [Fact]
    public void Donne_toujours_le_meme_hash_de_64_caracteres_pour_un_meme_token()
    {
        var token = _generator.Generate();

        var hash = _generator.Hash(token);

        Assert.Matches("^[0-9a-f]{64}$", hash);
        Assert.Equal(hash, _generator.Hash(token));
    }
}
