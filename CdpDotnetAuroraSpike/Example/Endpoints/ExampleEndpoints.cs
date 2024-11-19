using System.Diagnostics.CodeAnalysis;
using CdpDotnetAuroraSpike.Example.Services;

namespace CdpDotnetAuroraSpike.Example.Endpoints;

 [ExcludeFromCodeCoverage]
public static class ExampleEndpoints
{
    public static void UseExampleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("example", GetAll);
    }

    private static async Task<IResult> GetAll(IExamplePersistence examplePersistence)
    {
        var matches = examplePersistence.GetAllAsync();
        return Results.Ok(matches);
    }
}
