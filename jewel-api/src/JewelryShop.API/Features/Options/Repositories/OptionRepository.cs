using JewelryShop.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;

namespace JewelryShop.Features.Options.Repositories;

public class OptionRepository : Repository<Option>, IOptionRepository
{
    public OptionRepository(AppDbContext context) : base(context)
    {
    }
}
