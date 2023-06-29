# EndpointProviders
*The simplest way to add endpoints dynamically for Minimal API empowered by Dependency Injection principles.*

## Why another one?
Most of the libraries that target Minimal API functionality deal with instances of classes that can add Endpoints dynamically via a "marker" interface.
What is the approach then and why this is not fully practical? Let's see an example below.

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

What annoys me is that the role of the `IMarker` interface in the example above, is used only for automatic registration of all endpoints. 
The most practical way would be to ALSO use the `IMarker` to allow Dependency Injection within the `SampleWithEndPoints` without having to write EVERY time for EACH handler the `IRepository repo`.

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

For this library, each class the instance of which we want to add endpoints, should derive from the `EndpointProvider` abstract class. The `EndpointProvider` inherits the `IEndpointProvider` interface.
Each class that derives from `EndpointProvider` should override the `AddEndpoints` method. The above example should now be written as:

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

To add all endpoints from classes with the `IEndpointProvider` interface, we should use the `AddEndPointProviderFactory` before building the app, and the `AddEndpointsFromEndpointProviders` after building the app:

```cs
using EndpointProviders;
...

WebApplicationBuilder builder = WebApplication.CreateBuilder(args); 

//1st method: call before building the app
builder.AddEndpointProviderFactory(); 
...
WebApplication app = builder.Build();

//this adds the endpoints - the application must be first build
//the method gets as inputs any class that can identify its parent Assembly
//in the example below, the MarkerClass identifies the assembly

//2nd method: call after building the app. That's it!
app.AddEndpointsFromEndpointProviders(typeof(MarkerClass));
```

The `AndEndpointProviderFactory` method is responsible for the insertion of the endpoints.
The `AddEndpointsFromEndpointProviders` method collects and initializes all `IEndPointProvider` objects by passing the `IServiceProvider` to their constructor.

