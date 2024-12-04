using Netstr.Options;

namespace Netstr.Extensions
{
    public static class OptionsExtensions
    {
        public static IServiceCollection AddApplicationOptions<T>(this IServiceCollection services, string sectionName)
            where T: class
        {
            services
                .AddOptions<T>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection(sectionName).Bind(options));

            return services;
        }

        public static IServiceCollection AddApplicationsOptions(this IServiceCollection services)
        {
            return services
                .AddApplicationOptions<ConnectionOptions>("Connection")
                .AddApplicationOptions<RelayInformationOptions>("RelayInformation")
                .AddApplicationOptions<LimitsOptions>("Limits")
                .AddApplicationOptions<AuthOptions>("Auth")
                .AddApplicationOptions<CleanupOptions>("Cleanup");
        }
    }
}
