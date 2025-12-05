using Data.SmartAppt.SQL.Configs;
using Data.SmartAppt.SQL.Services;
using Data.SmartAppt.SQL.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Data.SmartAppt.SQL.Extensions
{
    public static class DataLayerServiceCollection
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string? connectionString)
        {
            services.Configure<DataBaseOptions>(opt => opt.ConnectionString = connectionString);
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IOpeningHoursRepository, OpeningHoursRepository>();
            services.AddScoped<IHolidayRepository, HolidayRepository>();

            return services;
        }
    }
}
