using Core.UseCases;
using Cronos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jobs;

public class ContentScanner : BackgroundService
{
    private readonly CronExpression _expression;
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly IServiceProvider _serviceProvider;

    public ContentScanner(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        
        _expression = CronExpression.Parse(configuration["ScanSchedule"]);
        _timeZoneInfo = TimeZoneInfo.Local;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await DoWork(stoppingToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
        }

        await ScheduleJob(stoppingToken);
    }

    protected async Task ScheduleJob(CancellationToken cancellationToken)
    {
        await DoWork(cancellationToken);
        while(!cancellationToken.IsCancellationRequested)
        {
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);

            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds > 0)
                {
                    await Task
                        .Delay(delay, cancellationToken)
                        .ConfigureAwait(false);
                }

                await DoWork(cancellationToken);
            }
        }
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<INotifyOfNewlyAddedMediaUseCase>();
            await useCase.Handle();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}