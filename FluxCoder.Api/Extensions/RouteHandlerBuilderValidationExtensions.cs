using FluxCoder.Api.Common.Filters;

namespace FluxCoder.Api.Extensions;

public static  class RouteHandlerBuilderValidationExtensions
{
    public static RouteHandlerBuilder WithRequestValidation<TRequest>(this RouteHandlerBuilder builder)
    {
        return builder
            .AddEndpointFilter<RequestValidationFilter<TRequest>>()
            .ProducesValidationProblem();
    }
}