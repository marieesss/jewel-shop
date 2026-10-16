using JewelryShop.Application.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;

namespace JewelryShop.Application.Features.Options;

public interface IOptionRepository : IRepository<Option>
{
}

public class OptionRepository : Repository<Option>, IOptionRepository
{
    public OptionRepository(AppDbContext context) : base(context)
    {
    }
}
