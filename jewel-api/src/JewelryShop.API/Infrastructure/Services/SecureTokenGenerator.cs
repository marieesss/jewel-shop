using System.Security.Cryptography;
using System.Text;
using JewelryShop.Common.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace JewelryShop.Infrastructure.Services;

public class SecureTokenGenerator : ISecureTokenGenerator
{
    private const int TokenSizeInBytes = 32;

    public string Generate()
        => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenSizeInBytes));

    public string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
