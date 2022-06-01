using Core.Domain;

namespace Core.Services;

public interface ISlackService
{
    /**
     * Send a message to a channel
     * @param mediaItem the media item to be sent
     */
    Task SendMediaItem(MediaItem item, string? webhookUrl = null, string responseType = "in_channel");

    Task SendSearchResult(List<MediaItem> item, string searchTerm, string? webhookUrl = null,
        string responseType = "in_channel");
    
    Task SendGroupedMediaItems(List<MediaItem> lst, string? webhookUrl = null, string responseType = "in_channel");
    
    Task SendSimpleMessage(string message, string? webhookUrl = null, string responseType = "in_channel");
}