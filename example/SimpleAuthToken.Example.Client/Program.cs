using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleAuthToken;
using SimpleAuthToken.Example.Client;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.SetMinimumLevel(LogLevel.Debug);
            loggerBuilder.AddFilter("Microsoft", LogLevel.Warning);
            loggerBuilder.AddFilter("System", LogLevel.Warning);
            loggerBuilder.AddConsole();
        });

        services.AddSingleton(new TokenConfig
        {
            PublicKey = "SimpleAuthToken.Example.Client",
            SecretKey = "422C2DF4-31A4-46CF-931E-07CC1F5E31BC",
            Issuer = "GBS"
        });
        
        services.AddHttpClient("DefaultHttpClient", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5001");
            client.Timeout = TimeSpan.FromMilliseconds(1000);
        });

        services.AddTransient<ApiClient>();
    });

var app = builder.Build();

await app.Services.GetService<ApiClient>()!.Run();