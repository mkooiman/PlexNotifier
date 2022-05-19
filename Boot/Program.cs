using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PlexNotifier;

public static class Program
{
    public static void Main(string[] args)
    {
        using (var host = CreateHostBuilder(args).Build())
        {
            host.Run();
        }
                    
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging( l=>
            {
                
                l.ClearProviders();
                l.AddConsole();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}