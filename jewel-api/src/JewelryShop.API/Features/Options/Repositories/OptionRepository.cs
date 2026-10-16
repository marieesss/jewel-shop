using JewelryShop.Application.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;

namespace JewelryShop.Application.Features.Options;

public class OptionRepository : Repository<Option>, IOptionRepository
{
    public OptionRepository(AppDbContext context) : base(context)
    {
    }
}
