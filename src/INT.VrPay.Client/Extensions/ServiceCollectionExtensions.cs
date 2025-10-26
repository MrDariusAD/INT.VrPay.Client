using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using INT.VrPay.Client.Configuration;

namespace INT.VrPay.Client.Extensions;

/// <summary>
/// Extension methods for configuring VrPay client services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the VrPay client to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing VrPay settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVrPayClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<VrPayConfiguration>(configuration.GetSection("VrPay"));

        // Register HttpClient
        services.AddHttpClient<IVrPayClient, VrPayClient>((serviceProvider, client) =>
        {
            var config = configuration.GetSection("VrPay").Get<VrPayConfiguration>();
            if (config != null)
            {
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            }
        });

        return services;
    }

    /// <summary>
    /// Adds the VrPay client with a custom configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure VrPay options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVrPayClient(
        this IServiceCollection services,
        Action<VrPayConfiguration> configureOptions)
    {
        // Configure options
        services.Configure(configureOptions);

        // Register HttpClient
        services.AddHttpClient<IVrPayClient, VrPayClient>((serviceProvider, client) =>
        {
            var config = new VrPayConfiguration();
            configureOptions(config);
            
            client.BaseAddress = new Uri(config.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        });

        return services;
    }
}
