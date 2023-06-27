using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EndpointProviders;

public static class EndpointProviderScanner
{
    private static IServiceScope? currentScope;

    public static WebApplication AddEndpointsFromEndpointProviders(this WebApplication app, params Type[] assemblyMarkers)
    {
        currentScope ??= app.Services.CreateScope();

        var services = currentScope.ServiceProvider;

        EndpointProviderFactory factory = services.GetRequiredService<EndpointProviderFactory>();

        List<IEndpointProvider> endpointsProviders = new();

        foreach (var marker in assemblyMarkers)
        {
            var types =
                marker.Assembly.ExportedTypes
                .Where(x => typeof(IEndpointProvider).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(factory.GetEndpointProvider);
            endpointsProviders.AddRange(types!);
        }

        //add the endpoints AFTER building the web app
        foreach (var provider in endpointsProviders)
            provider.AddEndpoints(app);
        return app;
    }

}