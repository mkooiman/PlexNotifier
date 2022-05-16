using Core.Domain;

namespace Core.Services;

public interface ISlackService
{
    Task SendMediaItem(MediaItem item);
}