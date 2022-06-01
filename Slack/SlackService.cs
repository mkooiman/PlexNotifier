using System.Text.Json;
using Core.Domain;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Slack.Webhooks;
using Slack.Webhooks.Blocks;
using Slack.Webhooks.Elements;
using Stubble.Core;
using File = System.IO.File;

namespace Slack;

internal sealed class SlackService: ISlackService
{

    private readonly string _channel;
    private readonly string _webhookUrl;
    private readonly ILogger<SlackService> _logger;

    private readonly string _searchTemplate = "SearchResult.json.mustache";
    private readonly string _episodeTemplate = "Episode.json.mustache";
    private readonly string _episodeListTemplate = "EpisodeList.json.mustache";
    private readonly string _movieTemplate = "Movie.json.mustache";
    private readonly string _messageTemplate = "Message.json.mustache";
    public SlackService(ILogger<SlackService> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _webhookUrl = configuration["Slack:WebhookUrl"];
        _channel = configuration["Slack:Channel"];
    }

    public async Task SendSearchResult(List<MediaItem> item, string searchTerm, string? webhookUrl = null,
        string responseType = "in_channel")
    {
        webhookUrl ??= _webhookUrl;
        var slackClient = new SlackClient(webhookUrl);

        var message = await ReadMessageTemplate(_searchTemplate, new
        {
            Channel = _channel,
            ResponseType = responseType,
            NrResults = item.Count,
            Query = searchTerm,
            Results = item
                .Select(i =>
                    new
                     {
                         i.Title,
                         i.Description,
                         Image = i.ImageUrl,
                         
                     }   
                    )
                .ToList()
            
        });
        //
        // string title;
        // if (item.ItemType == ItemType.Movie)
        // {
        //     string tagLine = item.TagLine == null ? "" :("_"+ item.TagLine+"_");
        //     title = $"*{item.Title}*,\n{tagLine}\nOn plex share *_{item.Server}_*";
        //     
        // }
        // else
        // {
        //     title = $"*{item.Show} {item.Season:D2}E{item.Episode:D2}*\nOn plex share *_{item.Server}_*";
        // }

        // slackMessage.Blocks = CreateSearchBlocks(title, item.ImageUrl, item.Title);
        
        var result = await slackClient
            .PostAsync(message)
            .ConfigureAwait(false);

    }

    public Task SendGroupedMediaItems(List<MediaItem> lst, string? webhookUrl = null, string responseType = "in_channel")
    {
        webhookUrl ??= _webhookUrl;
        var slackClient = new SlackClient(webhookUrl);

        var slackMessage = new SlackMessage
        {
            Channel = _channel,
            IconEmoji = Emoji.MovieCamera,
            Username = "PlexNotifier",
            ResponseType = responseType,
            
            
        };

        string title = $"*{lst.Count}* new media items";

        slackMessage.Blocks = CreateGroupedBlocks(title, lst);

        var result = slackClient
            .PostAsync(slackMessage)
            .ConfigureAwait(false);

        return result;
    }

    public async Task SendEpisode(MediaItem item, string? webhookUrl = null, string responseType = "in_channel")
    {
        
    }
    
    public async Task SendMediaItem(MediaItem item, string? webhookUrl = null, string responseType = "in_channel")
    {
        webhookUrl ??= _webhookUrl;
        var slackClient = new SlackClient(webhookUrl);

        var stars = (int) Math.Round(item.Rating / 2, MidpointRounding.ToEven);
        string? rating = null;
        
        var slackMessage = new SlackMessage
        {
            Channel = _channel,
            IconEmoji = item.ItemType == ItemType.Movie ? Emoji.MovieCamera: Emoji.Tv,
            Username = "PlexNotifier",
            ResponseType = responseType
        };

        string title;
        string description;
        if (item.ItemType == ItemType.Movie)
        {
            title = $"*I've just found a newly added Movie: {item.Title} on plex share _{item.Server}_*";
            string tagLine = item.TagLine == null ? "" :("_"+ item.TagLine+"_");
            description = $"*{item.Title}*\n{tagLine}\n{item.Description}";
            rating = ":star:";
            while (--stars > 0)
            {
                rating+= ":star:";
            }
        }
        else
        {
            title = $"*I've just found a newly added episode S{item.Season:D2}E{item.Episode:D2} of {item.Show} on plex share _{item.Server}_*";
            string tagLine = item.TagLine == null ? $"S{item.Season:D2}E{item.Episode:D2}" :$"_{item.TagLine}_";
            description = $"*{item.Show}: {item.Title}*\n{tagLine}\n{item.Description}";
        }

        slackMessage.Blocks = CreateNewlyAddedBlocks(title, description, rating, item.ImageUrl, item.Title);
        
      
        var result = await slackClient
            .PostAsync(slackMessage)
            .ConfigureAwait(false);
        
    }

    public async Task SendSimpleMessage(string message, string? webhookUrl = null, string responseType = "in_channel") 
    {
        webhookUrl ??= _webhookUrl;
        var slackClient = new SlackClient(webhookUrl);
        _logger.LogInformation("Sending message to slack on channel {channel}, to url {webhookUrl}", _channel, webhookUrl);

        var json = await ReadMessageTemplate("Message.json.mustache", new { Text = message, ResponseType = responseType })
            .ConfigureAwait(false);

        await slackClient
            .PostAsync(json)
            .ConfigureAwait(false);
        
    }

    private List<Block> CreateSearchBlocks(string title, string? image, string? alttext)
    {
        
        var blocks = new List<Block>()
        {
            new Section()
            {
                Text = new TextObject()
                {
                    Type = TextObject.TextType.Markdown,
                    Text = title
                },

                Accessory = new Webhooks.Elements.Image()
                {
                    ImageUrl = image,
                    AltText = alttext
                }
            }
        };
        
        return blocks;
    }
    
    private List<Block> CreateNewlyAddedBlocks(string title, string description, string? rating, string? image, string? alttext)
    {
        
        var blocks = new List<Block>()
        {
            new Section()
            {
                Text = new TextObject()
                {
                    Type = TextObject.TextType.Markdown,
                    Text = title
                }
            },
            new Section()
            {
                Text = new TextObject()
                {
                    Type = TextObject.TextType.Markdown,
                    Text = description
                },
                Accessory = new Webhooks.Elements.Image()
                {
                    ImageUrl = image,
                    AltText = alttext
                }
            }
            
        };
        if(rating != null)
        {
            blocks.Add(new Section()
            {
                Fields = new List<TextObject>()
                {
                    new()
                    {
                        Type = TextObject.TextType.Markdown,
                        Text = $"*Rating:*\n {rating}"
                    },
                }
            });
        }

        return blocks;
    }

    private async Task<SlackMessage?> ReadMessageTemplate(string fileName, object data)
    {
        
        var template = await File.ReadAllTextAsync(fileName);

        var result = await StaticStubbleRenderer.Instance
            .RenderAsync(template, data)
            .ConfigureAwait(false);

        return JsonSerializer.Deserialize<SlackMessage>(result);
    }
}