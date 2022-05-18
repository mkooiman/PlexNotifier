using Core.Domain;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Slack.Webhooks;
using Slack.Webhooks.Blocks;
using Slack.Webhooks.Elements;

namespace Slack;

internal sealed class SlackService: ISlackService
{

    private readonly string _channel;
    private readonly string _webhookUrl;
    public SlackService(IConfiguration configuration)
    {
        _webhookUrl = configuration["Slack:WebhookUrl"];
        _channel = configuration["Slack:Channel"];
    }
    
    public async Task SendMediaItem(MediaItem item)
    {
        var slackClient = new SlackClient(_webhookUrl);

        var stars = (int) Math.Round(item.Rating / 2, MidpointRounding.ToEven);
        string? rating = null;
        
        var slackMessage = new SlackMessage
        {
            Channel = _channel,
            IconEmoji = item.ItemType == ItemType.Movie ? Emoji.MovieCamera: Emoji.Tv,
            Username = "PlexNotifier"
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

        slackMessage.Blocks = CreateBlocks(title, description, rating, item.ImageUrl, item.Title);
        
      
        var result = await slackClient
            .PostAsync(slackMessage)
            .ConfigureAwait(false);
        
        Console.WriteLine(result);
    }

    private List<Block> CreateBlocks(string title, string description, string? rating, string? image, string? alttext)
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
}