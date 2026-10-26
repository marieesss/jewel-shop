using JewelryShop.Common.Exceptions;
using JewelryShop.Features.ChainLengths.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JewelryShop.Features.ChainLengths.Common;

public static class ChainLengthRepositoryExtensions
{
    // Le contrôle ExistsByLengthAsync n'est pas atomique : deux requêtes simultanées peuvent
    // le passer, et l'index unique de length_cm refuse la seconde à l'enregistrement.
    public static async Task SaveChangesOrConflictAsync(this IChainLengthRepository lengths, CancellationToken cancellationToken)
    {
        try
        {
            await lengths.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException(ChainLengthMessages.AlreadyExists);
        }
    }
}
