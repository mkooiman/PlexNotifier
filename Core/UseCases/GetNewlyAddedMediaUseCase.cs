using Core.Domain;
using Core.Repositories;
using Core.Services;

namespace Core.UseCases;

public interface IGetNewlyAddedMediaUseCase
{
    Task Handle();
}

internal sealed class GetNewlyAddedMediaUseCase : IGetNewlyAddedMediaUseCase
{
    private readonly IPlexService _plexService;
    private readonly IScanRepository _scanRepository;
    private readonly ISlackService _slackService;

    public GetNewlyAddedMediaUseCase(IPlexService plexService, ISlackService slackService,  IScanRepository scanRepository)
    {
        _plexService = plexService ?? throw new ArgumentNullException(nameof(plexService));
        _scanRepository = scanRepository ?? throw new ArgumentNullException(nameof(scanRepository));
        _slackService = slackService ?? throw new ArgumentNullException(nameof(slackService));
    }
    
    public async Task Handle()
    {
        var lastAdded = await _plexService
            .GetRecentlyAdded()
            .ConfigureAwait(false);
        
        var lastScan = await _scanRepository
            .GetLastScan()
            .ConfigureAwait(false);

        var newItems = lastAdded
            .Where(i => i.AddedAt > lastScan.Time)
            .ToList();
        
        foreach(var mi in newItems)
            await _slackService
                .SendMediaItem(mi)
                .ConfigureAwait(false);
        
        await _scanRepository
            .SetLastScan(new Scan(lastAdded.Max(i => i.AddedAt)))
            .ConfigureAwait(false);
    }
}