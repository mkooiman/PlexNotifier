using System.Net;
using Core.Domain;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plex.Api.Factories;
using Plex.Library.ApiModels.Accounts;
using Plex.Library.Factories;
using Plex.Mappers;
using Plex.ServerApi;
using Plex.ServerApi.Api;
using Plex.ServerApi.Clients;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Media;

namespace Plex;

public static class Module
{
    public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        var apiOptions = new ClientOptions
        {
            Product = "PlexNotify",
            DeviceName = Dns.GetHostName(),
            ClientId = "com.mkooiman.PlexNotify",
            Platform = "Web",
            Version = "v1"
        };

        services.AddSingleton(apiOptions);
        services.AddTransient<IPlexServerClient, PlexServerClient>();
        services.AddTransient<IPlexAccountClient, PlexAccountClient>();
        services.AddTransient<IPlexLibraryClient, PlexLibraryClient>();
        services.AddTransient<IApiService, ApiService>();
        services.AddTransient<IPlexFactory, PlexFactory>();
        services.AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>();

        services.AddScoped<PlexAccount>( x => 
            x.GetRequiredService<IPlexFactory>()
             .GetPlexAccount(configuration["Plex:Token"]));

        services.AddScoped<IPlexService, PlexService>();
    }
}