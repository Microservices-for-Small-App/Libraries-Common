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
    private const string RabbitMq = "RABBITMQ";
    private const string ServiceBus = "SERVICEBUS";

    public static IServiceCollection AddMassTransitWithMessageBroker(this IServiceCollection services, IConfiguration config,
            Action<IRetryConfigurator>? configureRetries = default)
    {
        var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

        switch (serviceSettings!.MessageBroker?.ToUpper())
        {
            case ServiceBus:
                services.AddMassTransitWithServiceBus(configureRetries);
                break;
            case RabbitMq:
            default:
                services.AddMassTransitWithRabbitMq(configureRetries);
                break;
        }

        return services;
    }

    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services,
        Action<IRetryConfigurator>? configureRetries = default)
    {
        services.AddMassTransit(configure =>
        {
            configure.AddConsumers(Assembly.GetEntryAssembly());

            configure.UseRabbitMqService(configureRetries);
        });

        services.AddMassTransitHostedService();

        return services;
    }

    public static IServiceCollection AddMassTransitWithServiceBus(this IServiceCollection services,
            Action<IRetryConfigurator>? configureRetries = default)
    {
        services.AddMassTransit(configure =>
        {
            configure.AddConsumers(Assembly.GetEntryAssembly());

            configure.UseAzureServiceBus(configureRetries);
        });

        services.AddMassTransitHostedService();

        return services;
    }

    public static void UseMessageBroker(this IServiceCollectionBusConfigurator configure, IConfiguration config,
            Action<IRetryConfigurator>? configureRetries = default)
    {
        var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

        switch (serviceSettings!.MessageBroker?.ToUpper())
        {
            case ServiceBus:
                configure.UseAzureServiceBus(configureRetries);
                break;
            case RabbitMq:
            default:
                configure.UseRabbitMqService(configureRetries);
                break;
        }
    }

    public static void UseRabbitMqService(this IServiceCollectionBusConfigurator configure,
        Action<IRetryConfigurator>? configureRetries = default)
    {
        configure.UsingRabbitMq((context, configurator) =>
        {
            var configuration = context.GetService<IConfiguration>();
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();

            configurator.Host(rabbitMQSettings!.Host);
            configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings!.ServiceName, false));

            configureRetries ??= (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));

            configurator.UseMessageRetry(configureRetries);
        });
    }

    public static void UseAzureServiceBus(this IServiceCollectionBusConfigurator configure,
            Action<IRetryConfigurator>? configureRetries = default)
    {
        configure.UsingAzureServiceBus((context, configurator) =>
        {
            var configuration = context.GetService<IConfiguration>();
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var serviceBusSettings = configuration.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();

            configurator.Host(serviceBusSettings!.ConnectionString);
            configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings!.ServiceName, false));

            configureRetries ??= (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));

            configurator.UseMessageRetry(configureRetries);
        });
    }

}
