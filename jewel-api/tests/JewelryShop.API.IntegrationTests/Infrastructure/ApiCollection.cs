namespace JewelryShop.API.IntegrationTests.Infrastructure;

/// <summary>
/// Un seul conteneur PostgreSQL et une seule API pour toutes les classes de la collection, qui s'exécutent l'une après l'autre.
/// La base n'est pas vidée entre les tests : chaque test crée ses propres données, avec un email unique.
/// </summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<JewelryShopApiFactory>
{
    public const string Name = "API";
}
