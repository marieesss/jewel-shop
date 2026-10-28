using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace JewelryShop.API.Tests.Fakes;

public class FakeHostEnvironment(string environmentName) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;
    public string ApplicationName { get; set; } = "JewelryShop.API";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
