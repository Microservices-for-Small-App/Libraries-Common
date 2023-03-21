using CommonLibrary.Settings;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CommonLibrary.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, Action<IRetryConfigurator>? configureRetries = default)
    {
        services.AddMassTransit(configure =>
        {
            configure.AddConsumers(Assembly.GetEntryAssembly());

            configure.UseRabbitMqService(configureRetries);
        });

        services.AddMassTransitHostedService();

        return services;
    }

    public static void UseRabbitMqService(this IServiceCollectionBusConfigurator configure, Action<IRetryConfigurator>? configureRetries = default)
    {
        configure.UsingRabbitMq((context, configurator) =>
        {
            var configuration = context.GetService<IConfiguration>();
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();

            configurator.Host(rabbitMQSettings!.Host);
            configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings!.ServiceName, false));

            if (configureRetries is null)
            {
                configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            }

            configurator.UseMessageRetry(configureRetries);
        });
    }

}
