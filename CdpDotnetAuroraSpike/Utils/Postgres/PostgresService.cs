using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace CdpDotnetAuroraSpike.Utils.Postgres;

[ExcludeFromCodeCoverage]
public abstract class PostgresService<T>
{
    protected readonly PostgresDbClientFactory.PostgresContext Context;

    protected readonly ILogger _logger;

    protected PostgresService(IPostgresDbClientFactory connectionFactory, string collectionName, ILoggerFactory loggerFactory)
    {
        Context = connectionFactory.getContext();
        var loggerName = GetType().FullName ?? GetType().Name;
        _logger = loggerFactory.CreateLogger(loggerName);
        
        try

        {
            _logger.LogInformation("Attempting connection...");
            Context.Database.OpenConnection();
            _logger.LogInformation("Connection established");
            Console.WriteLine("Connection successful.");

        }

        catch (Exception ex)

        {

            Console.WriteLine($"Connection failed: {ex.Message}");
            _logger.LogError($"Connection failed: {ex.Message}", ex);

        }

        finally

        {

            Context.Database.CloseConnection();
            _logger.LogError("Connection closed");

        }
    }

}
