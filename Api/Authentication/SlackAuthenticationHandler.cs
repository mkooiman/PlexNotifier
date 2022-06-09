using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Authentication;

internal sealed class SlackAuthenticationOptions : AuthenticationSchemeOptions
{
    public string SigningSecret { get; set; } = null!;
    public string? SignatureOverride { get; set; } = null;
}

internal sealed class SlackAuthenticationHandler: AuthenticationHandler<SlackAuthenticationOptions>
{
    private static readonly string VersionNumber = "v0";
    private static readonly string SignatureHeader = "X-Slack-Signature";
    private static readonly string TimestampHeader = "X-Slack-Request-Timestamp";
    
    
    public SlackAuthenticationHandler(
        IOptionsMonitor<SlackAuthenticationOptions> options, 
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
        
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogInformation("HandleAuthenticateAsync called");
        Request.EnableBuffering();
        if(!Request.Headers.ContainsKey(SignatureHeader) || !Request.Headers.ContainsKey(TimestampHeader))
        {
            Logger.LogWarning("Missing SignatureHeader!");
            return (AuthenticateResult.Fail("Missing headers"));
        }
        var expected = Request.Headers[SignatureHeader];

        using var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true);
        
        var body = await reader.ReadToEndAsync().ConfigureAwait(false);

        Request.Body.Position = 0;
        
        var timestamp = Request.Headers[TimestampHeader];
        
        var signatureBase = $"{VersionNumber}:{timestamp}:{body}";

        var hash = HMACSHA256.HashData( Encoding.UTF8.GetBytes(Options.SigningSecret), Encoding.UTF8.GetBytes(signatureBase));

        var toCheck = VersionNumber + "=" + Convert.ToHexString(hash);

        if (toCheck.Equals(expected, StringComparison.InvariantCultureIgnoreCase) )
        {
            Logger.LogInformation("Signatures match!");
        }
        else if (expected == Options.SignatureOverride)
        {
            Logger.LogInformation("Signature override used!");
        }
        else
        {
            Logger.LogWarning(
                              $"Signatures do not match!\n" +
                              $"Expected: {expected}\n" +
                              $"Actual: {toCheck}");
            return (AuthenticateResult.Fail("Signatures do not match"));
        }
        
        var identity = new ClaimsIdentity("slack");
        
        var principal = new ClaimsPrincipal(identity);
        
        AuthenticationTicket authenticationTicket = new AuthenticationTicket(principal, "slack");
        
        return (AuthenticateResult.Success(authenticationTicket));
    }
}