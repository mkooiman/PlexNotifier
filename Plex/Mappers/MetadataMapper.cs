using Core.Domain;
using Plex.ServerApi.PlexModels.Media;
using PlexNotifier.Shared.Util;

namespace Plex.Mappers;

internal static class MetadataMapper
{

    public static MediaItem Map(this Metadata metadata, ItemType type, string serverName, string baseUrl, string token)
    {
        var guid = metadata.Guid;
        var imageUrl = baseUrl + (metadata.Thumb ?? metadata.Art) + "?X-Plex-Token=" + token;
        var rating = metadata.AudienceRating;
        
        var showImageUrl = baseUrl + (metadata.GrandparentThumb ?? metadata.GrandparentArt) + "?X-Plex-Token=" + token;
        
        return new MediaItem(guid, metadata.Title, metadata.Summary, metadata.Tagline, imageUrl, rating,
            metadata.AddedAt.UnixTimestampToDate(), type, metadata.GrandparentTitle, showImageUrl, metadata.ParentIndex, metadata.Index, serverName);
    }
    
    
}