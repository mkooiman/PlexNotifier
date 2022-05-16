using Core.Repositories;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Slack;

public sealed class Module
{
    public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddScoped<ISlackService, SlackService>();
    }
}