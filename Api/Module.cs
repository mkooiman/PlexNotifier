using Api.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api;

public static class Module
{

    public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddAuthentication("slack")
            .AddScheme<SlackAuthenticationOptions, SlackAuthenticationHandler>("slack",
                op =>
                {
                    op.SigningSecret = configuration["Slack:SigningSecret"]; 
                    op.SignatureOverride = configuration["Slack:SignatureOverride"]; 
                });

        var builder = services.AddMvc();
        builder.AddControllersAsServices();

        services.AddControllers();
    }
    public static void Configure(IApplicationBuilder app)
    {

        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
