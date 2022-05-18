

using Core.UseCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var baseDir = Directory.GetParent(AppContext.BaseDirectory)!.FullName;

var files = Directory.GetFiles(baseDir, "appsettings*.json");

Array.Sort(files);

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(baseDir);

foreach (var file in files)
{
    configurationBuilder.AddJsonFile(file, true);
}
    
var configuration = configurationBuilder.Build();
    
ServiceCollection serviceCollection = new ServiceCollection();
serviceCollection.AddLogging();

serviceCollection.AddSingleton<IConfiguration>(configuration);

Core.Module.RegisterServices(configuration, serviceCollection);
Plex.Module.RegisterServices(configuration, serviceCollection);
Repository.Module.RegisterServices(configuration, serviceCollection);
Slack.Module.RegisterServices(configuration, serviceCollection);


var provider = serviceCollection.BuildServiceProvider();
var useCase = provider.GetRequiredService<INotifyOfNewlyAddedMediaUseCase>();
await useCase.Handle();