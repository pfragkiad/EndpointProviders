using System.Reflection;

namespace EndpointProviders;

//To allow the inclusion of WebApplication  the following FrameworkReference should be added.
/*
 <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="7.0.0" />
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
 */

public class EndpointProviderFactory
{
    private readonly IServiceProvider _provider;

    public EndpointProviderFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IEndpointProvider? GetEndpointProvider<T>() where T : class, IEndpointProvider
        =>GetEndpointProvider(typeof(T));

    public IEndpointProvider? GetEndpointProvider(Type t)
    {
        ConstructorInfo? constructor = t.GetConstructor(new Type[] { typeof(IServiceProvider) });
        return constructor?.Invoke(new[] { _provider }) as IEndpointProvider;
    }
 
}
