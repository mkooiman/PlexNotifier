using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Domain;
using Core.Services;
using JorgeSerrano.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlexNotifier.Shared.Util;
using Slack.Domain;
using Slack.Webhooks;
using Slack.Webhooks.Blocks;
using Slack.Webhooks.Elements;
using Stubble.Core;
using File = Slack.Webhooks.Blocks.File;
using Image = Slack.Webhooks.Blocks.Image;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Slack;

internal sealed class SlackService: ISlackService
{

    private readonly string _channel;
    private readonly string _webhookUrl;
    private readonly ILogger<SlackService> _logger;

    private readonly string _searchTemplate = AppDomain.CurrentDomain.BaseDirectory + "/Templates/SearchResult.json.mustache";
    private readonly string _episodeTemplate =  AppDomain.CurrentDomain.BaseDirectory + "/Templates/Episode.json.mustache";
    private readonly string _episodeListTemplate =  AppDomain.CurrentDomain.BaseDirectory + "/Templates/EpisodeList.json.mustache";
    private readonly string _movieTemplate =  AppDomain.CurrentDomain.BaseDirectory + "/Templates/Movie.json.mustache";
    private readonly string _messageTemplate =  AppDomain.CurrentDomain.BaseDirectory + "/Templates/Message.json.mustache";
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
            IconEmoji = Emoji.Question,
            ResponseType = responseType,
            NrResults = item.Count,
            Query = searchTerm,
            Results = item
                .Select(i =>
                    new
                     {
                         i.Title,
                         i.Description,
                         i.Server,
                         Image = i.ImageUrl,
                         
                     }   
                    )
                .ToList()
        });
        _logger.LogInformation($"Responding {message.AsJson()} to {_webhookUrl}");
        var result = await slackClient
            .PostAsync(message, false)
            .ConfigureAwait(false);
        _logger.LogInformation("Result: {result}", result);
    }

    public async Task SendGroupedMediaItems(List<MediaItem> lst, string? webhookUrl = null, string responseType = "in_channel")
    {
        webhookUrl ??= _webhookUrl;
        var slackClient = new SlackClient(webhookUrl);

        var message = await ReadMessageTemplate(_episodeListTemplate, new
        {
            Channel = _channel,
            ResponseType = responseType,
            IconEmoji = Emoji.Tv,
            SeriesTitle = lst[0].Show,
            SeriesThumb = lst[0].ShowImage,
            NrResults = lst.Count,
            lst[0].Server,
            Episodes = lst
                .Select(i =>
                    new
                    {
                        EpisodeTitle = i.Title,
                        EpisodeNumber = $"S{i.Season:D2}E{i.Episode:D2}",
                        i.Description,
                        Image = i.ImageUrl,
                         
                    }   
                )
                .OrderBy( i => i.EpisodeNumber)
                .ToList()
            
        });
        var result = await slackClient
            .PostAsync(message, false)
            .ConfigureAwait(false);
    }

    public async Task SendMediaItem(MediaItem item, string? webhookUrl = null, string responseType = "in_channel")
    {
        
        webhookUrl ??= _webhookUrl;
        var slackClient = new SlackClient(webhookUrl);
        SlackMessage? message; 
        if (item.ItemType == ItemType.Movie)
        {
            message = await CreateMovieMessage(item, responseType)
                .ConfigureAwait(false);
        }
        else
        {
            message = await CreateEpisodeMessage(item, responseType)
                .ConfigureAwait(false);
        }

        if (message != null)
        {
            
            await slackClient
                .PostAsync(message, false)
                .ConfigureAwait(false);
        }
    }

    private async Task<SlackMessage?> CreateEpisodeMessage(MediaItem item, string responseType)
    {
        return await ReadMessageTemplate(_episodeTemplate, new
        {
            Channel = _channel,
            ResponseType = responseType,
            IconEmoji = Emoji.Tv,
            SeriesTitle = item.Show,
            SeriesThumb = item.ShowImage,
            item.Server,
            Episode = new
            {
                EpisodeTitle = item.Title,
                EpisodeNumber = $"S{item.Season:D2}E{item.Episode:D2}",
                item.Description,
                Image = item.ImageUrl
            }
        });
       
    }

    private async Task<SlackMessage?> CreateMovieMessage(MediaItem item, string responseType)
    {
        
        var stars = (int) Math.Round(item.Rating / 2, MidpointRounding.ToEven);

        var rating = ":star:";
        
        while (--stars > 0)
        {
            rating+= ":star:";
        }
        return await ReadMessageTemplate(_movieTemplate, new
        {
            Channel = _channel,
            IconEmoji = Emoji.MovieCamera,
            ResponseType = responseType,
            item.Server,
            Item = new {
                item.Title,
                item.TagLine,
                Image = item.ImageUrl,
                item.Description,
                Rating = rating
            }
        });
    }

    public async Task SendSimpleMessage(string message, string? webhookUrl = null, string responseType = "in_channel") 
    {
        webhookUrl ??= _webhookUrl;
        var slackClient = new SlackClient(webhookUrl);
        _logger.LogInformation("Sending message to slack on channel {channel}, to url {webhookUrl}", _channel, webhookUrl);

        var json = await ReadMessageTemplate(_messageTemplate, new { Text = message, ResponseType = responseType })
            .ConfigureAwait(false);

        await slackClient
            .PostAsync(json, false)
            .ConfigureAwait(false);
        
    }

    private async Task<SlackMessage?> ReadMessageTemplate(string fileName, object data)
    {
        
        var template = await System.IO.File.ReadAllTextAsync(fileName);

        var result = await StaticStubbleRenderer.Instance
            .RenderAsync(template, data)
            .ConfigureAwait(false);

        var opts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
            PropertyNameCaseInsensitive = true,
        };
            
        opts.Converters.Add( new PolyJsonConverter<Block>("type", s =>
            {
                return s.ToLowerInvariant() switch
                {
                    "actions" => typeof(Actions),
                    "image" => typeof(Image),
                    "context" => typeof(Context),
                    "divider" => typeof(Divider),
                    "file" => typeof(File),
                    "text" => typeof(Text),
                    "input" => typeof(Input),
                    "section" => typeof(Section),
                    "header" => typeof(Header),
                    _ => throw new JsonException("invalid type!")
                };
            }));
        opts.Converters.Add(new PolyJsonConverter<Element>("type", s =>
        {
            return s.ToLowerInvariant() switch
            {
                "image" => typeof(Slack.Webhooks.Elements.Image),
                _ => throw new JsonException("invalid type!")
            };
        }));
        opts.Converters.Add(new JsonStringEnumMemberConverter());

        return JsonSerializer.Deserialize<SlackMessage>(result, opts);
    }
}