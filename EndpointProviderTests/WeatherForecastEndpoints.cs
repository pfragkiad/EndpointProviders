using EndpointProviders;

namespace EndpointProviderTests;

public class WeatherForecastEndpoints : EndpointProvider
{
    readonly WeatherForecastRepository _repo;

    public WeatherForecastEndpoints(IServiceProvider provider)
    {
        _repo = provider.GetRequiredService<WeatherForecastRepository>();
    }

    public override WebApplication AddEndpoints(WebApplication app)
    {
        app.MapGet("/weatherforecast", ForecastHandler)
        .WithName("GetWeatherForecast")
        .WithOpenApi();

        return app;
    }

    IResult ForecastHandler(int count)
    {
        if (count <= 0)
            return Results.BadRequest();

        //we use the repository here to get the forecasts
        var next = _repo.GetNext(count);
        return Results.Ok(next);
    }
}
