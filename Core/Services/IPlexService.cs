using Core.Domain;

namespace Core.Services;

public interface IPlexService
{ 
    Task<List<MediaItem>> GetRecentlyAdded(int count = 50);
}