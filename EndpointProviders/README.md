# EndpointProviders
*The simplest way to dynamically add endpoints in a Minimal API, leveraging Dependency Injection principles.*

## Why another one?
Most libraries targeting Minimal API functionality rely on class instances of classes that can dynamically add Endpoints using a "marker" interface.
But why is it not fully practical? Let's explore an example below.

```cs
public class SampleWithEndPoints : IMarker
{
	public void AddEndpoints (WebApplication app)
	{
		app.MapGet("/api/...", Handler1);
		app.MapGet("/api/...", Handler2);
		app.MapGet("/api/...", Handler3);
		app.MapPost("/api/...", Handler4);
		...
    }

	//c'mon it's not practical to add Dependency Injection for each method
	//wouldn't it be nice if we added the IRepository repo in the constructor of the class and make all these handlers not static?

	public static IResult Handler1(IRepository repo, int id) => {... _repo.DoSth(id); ...}
	
	public static IResult Handler2(IRepository repo, int id) => {...}

	public static IResult Handler3(IRepository repo, int id) => {...}

	public static IResult Handler4(IRepository repo, int id) => {...}

}

...

//and at some point register all endpoints via a method which scans all the IMarker classes in the assembly/app.
app.RegisterEndPoints(typeof(App));
```

What annoys me is that the role of the `IMarker` interface in the example above is limited to automatic registering all endpoints. 
A more practical approach would be to ALSO use the `IMarker` to enable Dependency Injection within the `SampleWithEndPoints` class, avoiding the need to repeatedly declare `IRepository repo` for each handler.

## The solution: EndpointProviders!

### How to install

Via tha Package Manager:
```powershell
Install-Package EndpointsProviders
```

Via the .NET CLI
```bat
dotnet add package EndpointsProviders
```

### How to use

For this library, every class whose instance we want to use for adding endpoints must derive from the `EndpointProvider` abstract class.
The `EndpointProvider` itself implements the `IEndpointProvider` interface.
Each class derived from `EndpointProvider` is required to override the `AddEndpoints` method. The example above can now be rewritten as:

```cs
public class SampleWithEndPoints : EndpointProvider
{
	readonly IRepository _repo;

	//Each EndpointProvider should have the constructor below.
	//The provider can then be used to create other instances via Dependency Injection in the constructor.
	public SampleWithEndPoints(IServiceProvider provider) : base(provider)
	{
		repo = provider.GetService<IRepository>();
	}

	public override WebApplication AddEndpoints (WebApplication app)
	{
		app.MapGet("/api/...", Handler1);
		app.MapGet("/api/...", Handler2);
		app.MapGet("/api/...", Handler3);
		...

		return app;
    }

	//we can now use every instance that we created, without having to inject the IRepository for each handler! 
	//and the handlers are not static

	public IResult Handler1(int id) => { ... _repo.DoSth(id); ...}
	
	public IResult Handler2(int id) => {...}

	public IResult Handler3(int id) => {...}
}
```

To register all endpoints from classes with the `IEndpointProvider` interface, use `AddEndPointProviderFactory` before building the app and `AddEndpointsFromEndpointProviders` after building the app:

```cs
using EndpointProviders;
...

WebApplicationBuilder builder = WebApplication.CreateBuilder(args); 

//1st method: call before building the app
builder.Services.AddEndpointProviderFactory(); 
...
WebApplication app = builder.Build();

//this adds the endpoints - the application must be first build
//the method gets as inputs any class that can identify its parent Assembly
//in the example below, the MarkerClass identifies the assembly

//2nd method: call after building the app. That's it!
app.AddEndpointsFromEndpointProviders(typeof(MarkerClass));
```

The `AndEndpointProviderFactory` method is responsible for inserting the endpoints.
The `AddEndpointsFromEndpointProviders` method collects and initializes all `IEndPointProvider` objects by injecting the `IServiceProvider` into their constructors.

## Example 1 - Simple example

Let's modify the classic `WeatherForecast` sample Minimal API project, to provide an explicit example.

We slightly modify the `WeatherForecast` class to a struct, as shown below (there is no particular reason for this, just my preference):

```cs
namespace EndpointProviderTests;

public readonly struct WeatherForecast
{
    public DateOnly Date { get; init; }

    public int TemperatureC { get; init; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; init; }
}
```

Now, let's build a repository that retrieves `WeatherForecast` instances:

```cs
namespace EndpointProviderTests;

public class WeatherForecastRepository
{
    string[] _summaries = new[] {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public List<WeatherForecast> GetNext(int count) =>
        Enumerable.
            Range(1, count).
            Select(index =>
                new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = _summaries[Random.Shared.Next(_summaries.Length)]
                }).
            ToList();
}
```

Now, let's add the `EndpointProvider` that will contain the endpoints to be added to the web app. Note that we include a constructor that accepts an `IServiceProvider` argument.
Through the passed provider, we inject the repository, which can now be used by any handler (in this case, we have only one).

```cs
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

    //note that we do not need to pass the repo here
    IResult ForecastHandler(int count)
    {
        if (count <= 0)
            return Results.BadRequest();

        //we use the repository here to get the forecasts
        var next = _repo.GetNext(count);
        return Results.Ok(next);
    }
}
```

