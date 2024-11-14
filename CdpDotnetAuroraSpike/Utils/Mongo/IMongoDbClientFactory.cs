using MongoDB.Driver;

namespace CdpDotnetAuroraSpike.Utils.Mongo;

public interface IMongoDbClientFactory
{
    IMongoClient GetClient();

    IMongoCollection<T> GetCollection<T>(string collection);
}