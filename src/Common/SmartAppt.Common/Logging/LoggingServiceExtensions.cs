using Microsoft.Extensions.DependencyInjection;

namespace SmartAppt.Common.Logging;

public static class LoggingServiceExtensions
{
    public static IServiceCollection AddCustomLogging(
        this IServiceCollection services)
    {
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        services.AddHostedService<LogBackgroundWriter>();
        return services;
    }
}
