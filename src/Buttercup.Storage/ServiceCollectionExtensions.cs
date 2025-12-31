using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.Storage;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds storage services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddButtercupStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection("Storage"));

        services.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(configuration.GetConnectionString("AzureBlobStorage"));
        });

        services.AddSingleton<IPhotoStorageService, AzureBlobPhotoStorageService>();

        return services;
    }
}
