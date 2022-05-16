using Core.Domain;

namespace Core.Services;

public interface ISlackService
{
    /**
     * Send a message to a channel
     * @param mediaItem the media item to be sent
     */
    Task SendMediaItem(MediaItem item);
}