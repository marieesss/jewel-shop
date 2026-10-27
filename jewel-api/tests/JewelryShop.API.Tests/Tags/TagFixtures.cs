using JewelryShop.Features.Tags.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JewelryShop.API.Tests.Tags;

public static class TagFixtures
{
    public static Tag Tag(long id, string name, TagCategory category = TagCategory.Theme) => new(name, category) { Id = id };

    /// <summary>Le tag a été supprimé entre sa vérification et l'enregistrement du lien.</summary>
    public static DbUpdateException ForeignKeyViolation()
        => new("Violation de clé étrangère.", new PostgresException("violates foreign key", "ERROR", "ERROR", PostgresErrorCodes.ForeignKeyViolation));
}
