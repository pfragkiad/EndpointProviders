using EndpointProviders;
using EndpointProviders.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndpointProviders;
public static class DependencyInjection
{
    public static IServiceCollection AddEndpointProviderFactory(this IServiceCollection services)
    {
        services.AddScoped<IEndpointProviderFactory, EndpointProviderFactory>(s=>new EndpointProviderFactory(s));

        return services;
    }
}

