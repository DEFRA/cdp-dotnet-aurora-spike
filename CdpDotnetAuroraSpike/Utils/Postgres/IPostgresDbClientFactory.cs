using Microsoft.EntityFrameworkCore;

namespace CdpDotnetAuroraSpike.Utils.Postgres;

public interface IPostgresDbClientFactory
{
    PostgresDbClientFactory.PostgresContext getContext();

}