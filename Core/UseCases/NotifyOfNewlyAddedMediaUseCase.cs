using Core.Domain;
using Core.Repositories;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.UseCases;

public interface INotifyOfNewlyAddedMediaUseCase
{
    /**
     * Notify Slack of any media added to the library since the last scan.
     */
    Task Handle();
}

internal sealed class NotifyOfNewlyAddedMediaUseCase : INotifyOfNewlyAddedMediaUseCase
{
    private readonly ILogger<NotifyOfNewlyAddedMediaUseCase> _logger;
    private readonly IPlexService _plexService;
    private readonly IScanRepository _scanRepository;
    private readonly ISlackService _slackService;
    private readonly CollapseSeriesSetting _collapse;

    public NotifyOfNewlyAddedMediaUseCase(
        ILogger<NotifyOfNewlyAddedMediaUseCase> logger,
        IConfiguration configuration,
        IPlexService plexService, 
        ISlackService slackService,  
        IScanRepository scanRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _plexService = plexService ?? throw new ArgumentNullException(nameof(plexService));
        _scanRepository = scanRepository ?? throw new ArgumentNullException(nameof(scanRepository));
        _slackService = slackService ?? throw new ArgumentNullException(nameof(slackService));
        
        if(Enum.TryParse<CollapseSeriesSetting>(configuration["CollapseSeries"], out var collapse))
        {
            _collapse = collapse;
        }
    }
    
    public async Task Handle()
    {
        var lastAdded = await _plexService
            .GetRecentlyAdded()
            .ConfigureAwait(false);
        
        var lastScan = await _scanRepository
            .GetLastScan()
            .ConfigureAwait(false);
        
        _logger.LogInformation($"Last added item was at: {lastScan?.Time}");
        
        
        var newItems = lastAdded
            .Where(i => i.AddedAt > lastScan.Time)
            .ToList();

        var grouped = newItems
            .GroupBy(ni => ni.ItemType == ItemType.Movie ? ni.Title : ni.Show);

        foreach (var group in grouped)
        {
            var lst = group.ToList();

            if (_collapse == CollapseSeriesSetting.Always ||
                (_collapse == CollapseSeriesSetting.OnMultiple &&
                 lst.Count > 1 &&
                 lst[0].ItemType == ItemType.Episode))
            {
                await _slackService
                    .SendGroupedMediaItems(lst)
                    .ConfigureAwait(false);
            }
            else
            {
                foreach (var mi in lst)
                {
                    await _slackService
                        .SendMediaItem(mi)
                        .ConfigureAwait(false);
                }
            }
            
        }
        await _scanRepository
            .SetLastScan(new Scan(lastAdded.Max(i => i.AddedAt)))
            .ConfigureAwait(false);
    }

}