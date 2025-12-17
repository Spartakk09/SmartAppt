using Business.SmartAppt.Services;
using Business.SmartAppt.Services.Implementation;
using Data.SmartAppt.SQL.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Business.SmartAppt.Extensions;

public static class BusinessLayerServiceCollection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, string? connectionString)
    {
        services.AddDataLayer(connectionString);

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBusinessService, BusinessService>();

        return services;
    }
}
