using Core.UseCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class Module
{
    public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddScoped<INotifyOfNewlyAddedMediaUseCase, NotifyOfNewlyAddedMediaUseCase>();
    }
}