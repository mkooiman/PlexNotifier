

using Core.UseCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)!.FullName)
    .AddJsonFile("appsettings.json", false)
    .AddJsonFile("appsettings.omv.json", true)
    .Build();

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