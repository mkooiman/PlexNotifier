using Core.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Repository;

public static class Module
{

    public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddScoped<IScanRepository, InFileScanRepository>();
    }
}