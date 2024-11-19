using CdpDotnetAuroraSpike.Example.Models;
using CdpDotnetAuroraSpike.Utils.Mongo;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using CdpDotnetAuroraSpike.Utils.Postgres;

namespace CdpDotnetAuroraSpike.Example.Services;

public interface IExamplePersistence{
    
   public PostgresDbClientFactory.Foo GetAllAsync();
}

/**
 * An example of how to persist data in MongoDB.
 * The base class `MongoService` provides access to the db collection as well as providing helpers to
 * ensure the indexes for this collection are created on startup.
 */

public class ExamplePersistence(IPostgresDbClientFactory connectionFactory, ILoggerFactory loggerFactory)
    : PostgresService<ExampleModel>(connectionFactory, "example", loggerFactory), IExamplePersistence
{

   [ExcludeFromCodeCoverage]
   public PostgresDbClientFactory.Foo GetAllAsync()
   {
       var foo = Context.Foos.First();
       Console.WriteLine(foo?.Bar);
       Context.SaveChanges();
       return foo;
   }


}
