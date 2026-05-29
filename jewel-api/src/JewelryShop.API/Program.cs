using DotNetEnv;
using JewelryShop.API;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");

Env.Load(envPath, new LoadOptions(setEnvVars: true, clobberExistingVars: false));

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.ConfigureServices();

var app = builder.Build();

app.ConfigurePipeline();

app.Run();
