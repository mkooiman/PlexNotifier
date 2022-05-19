namespace PlexNotifier;

public class Startup
{

    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_configuration);
        Api.Module.RegisterServices(_configuration, services);
        Core.Module.RegisterServices(_configuration, services);
        Plex.Module.RegisterServices(_configuration, services);
        Repository.Module.RegisterServices(_configuration, services);
        Slack.Module.RegisterServices(_configuration, services);
        Jobs.Module.RegisterServices(_configuration, services);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        Api.Module.Configure(app);
    }

}