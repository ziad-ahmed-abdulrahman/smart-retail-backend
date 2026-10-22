using Microsoft.Extensions.DependencyInjection;

namespace SmartRetail.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Application layer services (Services, Validators, AutoMapper, etc.)
        return services;
    }
}

