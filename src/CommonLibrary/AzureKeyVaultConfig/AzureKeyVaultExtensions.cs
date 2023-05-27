using Azure.Identity;
using CommonLibrary.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CommonLibrary.AzureKeyVaultConfig;

public static class Extensions
{
    public static IHostBuilder ConfigureAzureKeyVault(this IHostBuilder builder)
    {
        return builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            if (context.HostingEnvironment.IsProduction())
            {
                var configuration = configurationBuilder.Build();

                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

                _ = configurationBuilder.AddAzureKeyVault(
                        new Uri($"https://{serviceSettings!.KeyVaultName}.vault.azure.net/"),
                        new DefaultAzureCredential()
                    );
            }
        });
    }
}