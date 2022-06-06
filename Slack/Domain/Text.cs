using Slack.Webhooks;

namespace Slack.Domain;

public sealed class Text: Block
{
    public Text() : base(BlockType.Text)
    {
    }
}