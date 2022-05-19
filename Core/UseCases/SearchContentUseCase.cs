using Core.Services;
using Microsoft.Extensions.Logging;

namespace Core.UseCases;

public interface ISearchContentUseCase
{
    Task Handle(string searchTerm, string callbackUrl);
}

internal sealed class SearchContentUseCase : ISearchContentUseCase
{
    
    private readonly IPlexService _plexService;
    private readonly ISlackService _slackService;
    private readonly ILogger<SearchContentUseCase> _logger;

    public SearchContentUseCase(
        ILogger<SearchContentUseCase> logger,
        IPlexService plexService,
        ISlackService slackService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _plexService = plexService ?? throw new ArgumentNullException(nameof(plexService));
        _slackService = slackService ?? throw new ArgumentNullException(nameof(slackService));
        
    }
    
    public async Task Handle(string searchTerm, string callbackUrl)
    {
        if (searchTerm.Length < 3)
        {
            await _slackService
                .SendSimpleMessage("Search term must be at least 3 characters long",callbackUrl, "ephemeral")
                .ConfigureAwait(false);
            
            return;
        }
        var result = await _plexService
            .SearchContent(searchTerm)
            .ConfigureAwait(false);

        if (result.Count > 0)
        {
            _logger.LogInformation("Found {0} results", result.Count);
            await _slackService
                .SendSimpleMessage("I've found the following results:", callbackUrl, "ephemeral")
                .ConfigureAwait(false);
        }
        else
        {
            _logger.LogInformation("No results found");
            await _slackService
                .SendSimpleMessage("I couldn't find anything :(", callbackUrl, "ephemeral")
                .ConfigureAwait(false);
        }
        foreach (var mediaItem in result)
        {
            await _slackService
                .SendMediaItem(mediaItem, callbackUrl, "ephemeral")
                .ConfigureAwait(false);
        }
    }
}