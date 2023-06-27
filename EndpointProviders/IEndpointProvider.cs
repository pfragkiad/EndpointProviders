using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EndpointProviders;

public interface IEndpointProvider
{
    // IEndpointRouteBuilder AddEndpoints(IEndpointRouteBuilder app);
    WebApplication AddEndpoints(WebApplication app);
   // IServiceProvider ImplementServices(IServiceProvider provider);
}
