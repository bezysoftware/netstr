namespace Netstr.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSingleton<TService1, TService2, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService1, TService2
            where TService1 : class
            where TService2 : class
        {
            services.AddSingleton<TService1, TImplementation>();
            services.AddSingleton<TService2, TImplementation>(x => (TImplementation)x.GetRequiredService<TService1>());
        }
    }
}
