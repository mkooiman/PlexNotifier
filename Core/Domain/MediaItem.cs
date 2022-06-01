namespace Core.Domain;

public sealed record MediaItem(string Id, string Title, string? Description,string? TagLine, string? ImageUrl,
    double Rating, DateTime AddedAt, ItemType ItemType, string? Show, string? ShowImage, int? Season, int Episode, string Server)
{
}

public enum ItemType
{
    Movie,
    Episode
}