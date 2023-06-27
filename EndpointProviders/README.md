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

	public static IResult Handler1(IRepository repo, int id) => {...}
	
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

For this library, each class the instance of which we want to add endpoints, should derive the `EndpointProvider` abstract class. The `EndpointProvider` inherits the `IEndpointProvider` interface.
Each class that derives from `EndpointProvider` should override the `AddEndpoints` method
The above example should now be written as:

```cs
public class SampleWithEndPoints : EndpointProvider
{
	readonly IRepository _repo;

	//Each EndpointProvider should have exactly the constructor below. The provider can then be used to create other instances via Dependency Injection in the constructor.
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

	//we can now use every instance that we created, without having to inject for each handler the IRepository! 
	//and the handlers are not static now!

	public IResult Handler1(int id) => { ...	_repo.DoSth(id); ...}
	
	public IResult Handler2(int id) => {...}

	public IResult Handler3(int id) => {...}
}
```

To add all endpoints from classes with the `IEndpointProvider` interface, we should use 2 methods as shown below:

```cs
using EndpointProviders;
...

WebApplicationBuilder builder = WebApplication.CreateBuilder(args); 

builder.AddEndpointProviderFactory(); //1st method: call before building the app
...
WebApplication app = builder.Build();

//this adds the endpoints - the application must be first build
//the method gets as inputs any class that can identify its parent Assembly
//in the example below, the MarkerClass identifies the assembly
app.AddEndpointsFromEndpointProviders(typeof(MarkerClass)); //2nd method: call after building the app. That's it!
```

The `AndEndpointProviderFactory` method is reponsible for the services registration and passing each `IServiceProvider` to each `EndpointProvider` class constructor.
The final method `AddEndpointsFromEndpointProviders` is the method that finally adds the endpoints to the current Web API app.

