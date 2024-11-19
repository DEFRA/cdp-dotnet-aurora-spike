using System.Diagnostics;
using CdpDotnetAuroraSpike.Example.Endpoints;
using CdpDotnetAuroraSpike.Example.Services;
using CdpDotnetAuroraSpike.Utils;
using CdpDotnetAuroraSpike.Utils.Http;
using CdpDotnetAuroraSpike.Utils.Logging;
using CdpDotnetAuroraSpike.Utils.Mongo;
using FluentValidation;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;
using Amazon;
using Amazon.Internal;
using CdpDotnetAuroraSpike.Utils.Postgres;
using Npgsql;

//-------- Configure the WebApplication builder------------------//


var app = CreateWebApplication(args);
await app.RunAsync();

static void RunLiquibase(WebApplicationBuilder _builder)
{
    try
    {
        string? postgresUri = _builder.Configuration.GetValue<string>("Postgres:JdbcUri");
        // Set the Liquibase command you want to run (e.g., 'update')
        string liquibaseCommand = "update"; // Other options: 'rollback', 'status', etc.

        // Set the path to your Liquibase executable (make sure it's in the system PATH)
        string liquibasePath = "/usr/local/bin/liquibase"; // or full path to the liquibase executable

        // Set the database connection details and changelog file
        string changelogFile = "dbchangelog.xml"; // Path to your changelog file

        // Construct the command to run
        string arguments = $"--url={postgresUri} --changeLogFile={changelogFile} {liquibaseCommand}";

        // Start the Liquibase process
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = liquibasePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process? process = Process.Start(startInfo))
        {
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Console.WriteLine($"Liquibase Output: {output}");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Liquibase Error: {error}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error running Liquibase, exiting: {ex.Message}");
        Environment.Exit(1);
    }
}

[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
    var _builder = WebApplication.CreateBuilder(args);

    ConfigureWebApplication(_builder);

    var _app = BuildWebApplication(_builder);

    return _app;
}

[ExcludeFromCodeCoverage]
static void ConfigureWebApplication(WebApplicationBuilder _builder)
{
    _builder.Configuration.AddEnvironmentVariables();

    var logger = ConfigureLogging(_builder);

    // Load certificates into Trust Store - Note must happen before Mongo and Http client connections
    _builder.Services.AddCustomTrustStore(logger);

    // RunLiquibase(_builder);

    // ConfigureMongoDb(_builder);
    ConfigurePostgresDb(_builder, logger);

    ConfigureEndpoints(_builder);

    _builder.Services.AddHttpClient();

    // calls outside the platform should be done using the named 'proxy' http client.
    _builder.Services.AddHttpProxyClient(logger);

    _builder.Services.AddValidatorsFromAssemblyContaining<Program>();
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder _builder)
{
    _builder.Logging.ClearProviders();
    var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(_builder.Configuration)
        .Enrich.With<LogLevelMapper>()
        .Enrich.WithProperty("service.version", Environment.GetEnvironmentVariable("SERVICE_VERSION"))
        .CreateLogger();
    _builder.Logging.AddSerilog(logger);
    logger.Information("Starting application");
    return logger;
}

[ExcludeFromCodeCoverage]
static void ConfigureMongoDb(WebApplicationBuilder _builder)
{
    _builder.Services.AddSingleton<IMongoDbClientFactory>(_ =>
        new MongoDbClientFactory(_builder.Configuration.GetValue<string>("Mongo:DatabaseUri")!,
            _builder.Configuration.GetValue<string>("Mongo:DatabaseName")!));
}

[ExcludeFromCodeCoverage]
static void ConfigurePostgresDb(WebApplicationBuilder _builder, Logger logger)
{
    // _builder.Services.AddDbContext<PostgresDbClientFactory.PostgresContext>(options =>
    // options.UseNpgsql(_builder.Configuration.GetValue<string>("Postgres:DatabaseUri")));

    var connString = _builder.Configuration.GetValue<string>("Postgres:DotNetUri");
    var useIamAuthentication = _builder.Configuration.GetValue<bool>("Postgres:UseIamAuthentication");

    if (useIamAuthentication)
    if (useIamAuthentication)
    {
        logger.Information("Generating auth token...");
        var dbUserName = "cdp-dotnet-aurora-spike";
        var password = Amazon.RDS.Util.RDSAuthTokenGenerator.GenerateAuthToken(RegionEndpoint.EUWest2,
                "phil-test.cluster-c5kyrfhzgpe4.eu-west-2.rds.amazonaws.com", 5432, dbUserName);


        connString = $"{connString}User ID={dbUserName};Password={password};";
    }
    
    // Create a new connection
    using var conn = new NpgsqlConnection(connString);
    try
    {

        logger.Information("Connecting to the database: " + connString);
        // Open the connection
        conn.Open();
        logger.Information("Connected to the database successfully!");

        // Example query: Fetching data from a table
        using var cmd = new NpgsqlCommand("SELECT version();", conn);
        var version = cmd.ExecuteScalar()?.ToString();
        logger.Information($"PostgreSQL version: {version}");
    }
    catch (Exception ex)
    {
        logger.Error(ex, $"Error: {ex.Message}");
    }

    _builder.Services.AddSingleton<IPostgresDbClientFactory>(_ =>
        new PostgresDbClientFactory(_builder.Configuration.GetValue<string>("Postgres:DotNetUri")!));
}

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplicationBuilder _builder)
{
    // our Example service, remove before deploying!
    _builder.Services.AddSingleton<IExamplePersistence, ExamplePersistence>();

    _builder.Services.AddHealthChecks();
}

[ExcludeFromCodeCoverage]
static WebApplication BuildWebApplication(WebApplicationBuilder _builder)
{
    var app = _builder.Build();

    app.UseRouting();
    app.MapHealthChecks("/health");

    // Example module, remove before deploying!
    app.UseExampleEndpoints();

    return app;
}