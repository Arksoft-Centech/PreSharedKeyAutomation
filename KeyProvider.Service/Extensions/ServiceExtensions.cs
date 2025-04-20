using KeyProvider.Service.Contracts;
using KeyProvider.Service.Implementations.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace KeyProvider.Service.Extensions
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public static class ServiceExtensions
    {
        public static void RegisterDIServices(this IServiceCollection services)
        {
            services.AddSingleton<IKeyHolderService, KeyHolderService>();
            services.AddScoped<IKeyProviderService, KeyProviderService>();
            services.AddScoped<IDataService, DataService>();
        }
    }
}
