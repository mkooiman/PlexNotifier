using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs;

public class Module
{

    public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddHostedService<ContentScanner>();
    }
}