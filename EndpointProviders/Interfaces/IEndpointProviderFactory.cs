namespace EndpointProviders.Interfaces
{
    public interface IEndpointProviderFactory
    {
        IEndpointProvider? GetEndpointProvider(Type t);
    }
}