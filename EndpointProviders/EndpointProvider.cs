using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EndpointProviders;

public abstract class EndpointProvider : IEndpointProvider
{
    protected IServiceProvider? _provider;

    public EndpointProvider(IServiceProvider provider)
    {
        _provider = provider;
    }

    public abstract WebApplication AddEndpoints(WebApplication app);

    //public virtual IServiceProvider ImplementServices(IServiceProvider provider)
    //{
    //    //_provider = provider;
    //    return provider;
    //}
}
