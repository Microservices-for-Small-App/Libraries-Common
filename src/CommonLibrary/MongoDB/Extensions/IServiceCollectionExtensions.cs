using Catalog.Repositories;
using CommonLibrary.Entities;
using CommonLibrary.Interfaces;
using CommonLibrary.Settings;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace CommonLibrary.MongoDB.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

        _ = services.AddSingleton(serviceProvider =>
        {
            return new MongoClient(serviceProvider.GetService<MongoDbSettings>()?.ConnectionString)
                        .GetDatabase(serviceProvider.GetService<ServiceSettings>()?.ServiceName);
        });

        return services;
    }

    public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services)
        where T : IEntity
    {
        _ = services.AddSingleton<IRepository<T>>(serviceProvider =>
        {
            return new MongoRepository<T>(serviceProvider.GetService<IMongoDatabase>()!,
                                            serviceProvider.GetService<MongoDbCollectionSettings>()?.Name!);
        });

        return services;
    }
}
