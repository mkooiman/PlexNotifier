using Core.Domain;

namespace Core.Services;

public interface IPlexService
{ 
    /**
     * Retrieves all media from the plex server that is marked as recently added
     * @param int count The maximum number of items to retrieve
     * @return List<MediaItem> The list of media items
     */
    Task<List<MediaItem>> GetRecentlyAdded(int count = 50);

    Task<List<MediaItem>> SearchContent(string searchTerm);
}