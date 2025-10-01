using FluxCoder.Api.Endpoints;

namespace FluxCoder.Api.Extensions;

public static class EndpointsMappingExtension
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthEndpoints();
        endpoints.MapStreamEndpoints();
        return endpoints;
    }
}