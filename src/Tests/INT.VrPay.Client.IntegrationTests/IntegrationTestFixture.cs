using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using INT.VrPay.Client.Configuration;
using INT.VrPay.Client.Extensions;
using INT.VrPay.Client.Models;

namespace INT.VrPay.Client.IntegrationTests;

/// <summary>
/// Base fixture for integration tests.
/// </summary>
public class IntegrationTestFixture : IDisposable
{
    public ServiceProvider ServiceProvider { get; }
    public IVrPayClient VrPayClient { get; }
    public IConfiguration Configuration { get; }
    public bool IsConfigured { get; }

    public IntegrationTestFixture()
    {
        // Build configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Check if configured
        var entityId = Configuration["VrPay:EntityId"];
        var accessToken = Configuration["VrPay:AccessToken"];
        
        IsConfigured = !string.IsNullOrWhiteSpace(entityId) && 
                       !string.IsNullOrWhiteSpace(accessToken) &&
                       entityId != "dummy-entity-id" &&
                       accessToken != "Bearer dummy-token";

        // Build service provider with dummy config if not configured
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        if (IsConfigured)
        {
            services.AddVrPayClient(Configuration);
        }
        else
        {
            // Add with dummy configuration for validation tests
            services.AddVrPayClient(config =>
            {
                config.BaseUrl = "https://test.vr-pay-ecommerce.de/";
                config.EntityId = "dummy-entity-id";
                config.AccessToken = "Bearer dummy-token";
                config.UseTestMode = true;
                config.TestModeValue = TestMode.External;
            });
        }

        ServiceProvider = services.BuildServiceProvider();
        VrPayClient = ServiceProvider.GetRequiredService<IVrPayClient>();
    }

    public void Dispose()
    {
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
