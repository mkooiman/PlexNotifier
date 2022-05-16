# PlexNotifier
This is a small tool that is meant to notify of new content on a plex server in a slack channel

## Configuration 
Configuration can be done by editing the appsettings.json file, currently only a few parameters need to be configured 
```
{
    "Plex": {
        "Url": "[plex url]",
        "Token": "[plex token]"
    },
    "Slack":{
        "WebhookUrl": "[slack webhook url]",
        "Channel": "[slack channel]"

    }
 }
```
The Plex section contains the following:
| key | value |
|===|===|
| Plex.Url | The (public) url where your Plex server can be reached, this is required for the thumbnail to be retrieved|
| Plex.Token* | The Plex API Token |
| Slack.WebhookUrl | The webhook url retrieved from Slack | 
| Slack.Channel | The channel to post to in Slack | 


*) More details on the plex token at: https://support.plex.tv/articles/204059436-finding-an-authentication-token-x-plex-token/
**) https://slack.com/help/articles/115005265063-Incoming-webhooks-for-Slack
