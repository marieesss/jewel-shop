namespace JewelryShop.Features.Tags.Common;

/// <summary>Type d'article dont on compte les tags, valeur du paramètre <c>kind</c> de <c>GET /api/tags</c>.</summary>
public static class TagCountKinds
{
    public const string Chain = "chain";
    public const string Charm = "charm";

    public static bool IsValid(string? value) => value is Chain or Charm;
}
