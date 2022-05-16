using AutoMapper;
using Core.Domain;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Plex.Library.ApiModels.Accounts;
using Plex.Library.ApiModels.Libraries;
using Plex.Mappers;
using Plex.ServerApi.PlexModels.Media;

namespace Plex;

internal sealed class PlexService: IPlexService
{
    private readonly PlexAccount _account;
    private readonly string _baseUrl;
    private readonly string _token;

    public PlexService(PlexAccount account, IConfiguration configuration)
    {
        _account = account ?? throw new ArgumentNullException(nameof(account));
        _baseUrl = configuration["Plex:Url"];
        _token = configuration["Plex:Token"];
    }
        
    public async Task<List<MediaItem>> GetRecentlyAdded( int count = 50)
    {
        var servers = this._account.Servers().Result;
        
        var myServers = servers.Where(c => c.Owned == 1);

        var items = new List<MediaItem>();
        
        foreach (var server in myServers)
        {
            var libraries = await server
               .Libraries()
               .ConfigureAwait(false);

            foreach (var libraryBase in libraries)
            {
                
                
                if(libraryBase is MovieLibrary ml)
                {
                    var added = await ml
                        .RecentlyAdded(0, count)
                        .ConfigureAwait(false);
                    
                    items.AddRange(added.Media
                        .Select(a => a.Map( ItemType.Movie, server.FriendlyName , _baseUrl, _token ))
                        .ToList());
                }
                else if (libraryBase is ShowLibrary sl)
                {
                    var added = await sl.RecentlyAddedEpisodes();
                    
                    items.AddRange(added.Media
                        .Select(a => a.Map(ItemType.Episode, server.FriendlyName, _baseUrl , _token ))
                        .ToList());

                }
            }
        }

        items.Sort((i, j) => j.AddedAt.CompareTo(i.AddedAt));
        
        return items
            .Take(count)
            .ToList();
    }
}