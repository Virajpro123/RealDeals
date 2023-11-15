using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using RealDealsAPI.Entities;
using RealDealsAPI.Helpers;
using RealDealsAPI.Repositories;
using RealDealsAPI.Services;
using System.Threading.RateLimiting;

namespace RealDealsAPI
{
    public static class RegisterServices
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IMovieDataAccessService, MovieDataAccessService>();
            services.AddTransient<IExternalApiService, ExternalApiService>();
            services.AddTransient<IMovieRepository, MovieRepository>();
            services.AddTransient<MovieDTOComparer>();
            return services;
        }

        public static IServiceCollection AddSettings(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
            builder.Configuration.AddConfiguration(config);

            var settings = builder.Configuration.Get<Settings>();
            builder.Services.AddSingleton(settings);
            return services;
        }

        public static IServiceCollection AddRateLimitting(this IServiceCollection services)
        {
            services.AddRateLimiter(_ => _
             .AddFixedWindowLimiter(policyName: "fixed", options =>
             {
                 options.PermitLimit = 10;
                 options.Window = TimeSpan.FromSeconds(10);
                 options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                 options.QueueLimit = 2;
             }));
            return services;
        }
    }
}
