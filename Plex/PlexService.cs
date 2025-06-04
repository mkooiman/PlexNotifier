using Core.Domain;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Plex.Library.ApiModels.Accounts;
using Plex.Library.ApiModels.Libraries;
using Plex.Library.ApiModels.Servers;
using Plex.Mappers;

namespace Plex;

internal sealed class PlexService: IPlexService
{
    private readonly PlexAccount _account;
    private readonly string _token;
    private readonly bool _ownedOnly = false;
    private readonly List<string> _serverBlacklist;
    public PlexService(PlexAccount account, IConfiguration configuration)
    {
        _account = account ?? throw new ArgumentNullException(nameof(account));
        _token = configuration["Plex:Token"] ?? throw new ArgumentException("Missing Plex Token");
        _serverBlacklist = configuration.GetSection("Plex:ServerBlacklist").Get<List<string>>() ?? new List<string>();
        if (bool.TryParse(configuration["Plex:OwnedOnly"], out bool ownedOnly))
        {
            _ownedOnly = ownedOnly;
        }

    }
        
    public async Task<List<MediaItem>> GetRecentlyAdded( int count = 50)
    {
        var servers = this._account.Servers().Result;
        
        if(_ownedOnly)
        {
            servers = servers.Where(x => x.Owned == 1).ToList();
        }
        var items = new List<MediaItem>();
        
        foreach (var server in servers)
        {
            if(_serverBlacklist.Any(entry =>
                   string.Equals(entry, server.Name, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(entry, server.FriendlyName, StringComparison.OrdinalIgnoreCase)))
                continue;
            
            var libraries = await server
               .Libraries()
               .ConfigureAwait(false);
            var serverAddress = server.Scheme + "://" + server.Address + ":" + server.Port;
            foreach (var libraryBase in libraries)
            {
                
                if(libraryBase is MovieLibrary ml)
                {
                    var added = await ml
                        .RecentlyAdded(0, count)
                        .ConfigureAwait(false);
                    
                    items.AddRange(added.Media
                        .Select(a => a.Map( ItemType.Movie, server.FriendlyName , serverAddress, _token ))
                        .ToList());
                }
                else if (libraryBase is ShowLibrary sl)
                {
                    var added = await sl.RecentlyAddedEpisodes();
                    
                    items.AddRange(added.Media
                        .Select(a => a.Map(ItemType.Episode, server.FriendlyName, serverAddress , server.AccessToken ))
                        .ToList());

                }
            }
        }

        items.Sort((i, j) => j.AddedAt.CompareTo(i.AddedAt));
        
        return items
            .Take(count)
            .ToList();
    }

    public async Task<List<MediaItem>> SearchContent(string searchTerm)
    {
        var servers = await _account
            .Servers()
            .ConfigureAwait(false);
        
        servers = servers
            .Where(x => !_serverBlacklist.Any(entry =>
                string.Equals(entry, x.Name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entry, x.FriendlyName, StringComparison.OrdinalIgnoreCase)))
            .ToList();


        var tasks = servers
            .Select( x=> Search(x, searchTerm))
            .ToList();
        
        await Task
            .WhenAll(tasks)
            .ConfigureAwait(false);
        
        return tasks.SelectMany(task => task.Result).ToList();
    }

    private async Task<List<MediaItem>> Search(Server server, string searchTerm)
    {
        var result = await server
            .HubLibrarySearch(searchTerm)
            .ConfigureAwait(false);
        var hubs = result.Hub.Where(h => h.Metadata != null).ToList();
        if(hubs.Count == 0)
        {
            return new List<MediaItem>();
        }
        var serverAddress = server.Scheme + "://" + server.Address + ":" + server.Port;
        return hubs
            .SelectMany(h => h.Metadata
                .Select(m => m.Map(m.GrandparentArt == null ? ItemType.Movie : ItemType.Episode,
                server.FriendlyName, serverAddress, server.AccessToken)))
            .ToList();


    }
}