using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api;

public static class Module
{

    public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        
        var builder = services.AddMvc();
        builder.AddControllersAsServices();

        services.AddControllers();
    }
    public static void Configure(IApplicationBuilder app)
    {

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
