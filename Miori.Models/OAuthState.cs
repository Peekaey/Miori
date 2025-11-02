using Miori.Models.Enums;

namespace Miori.Models;

public class OAuthState
{
    public ulong DiscordUserId { get; set; }
    
    public long IssuedAt { get; set; }
    public string Nonce { get; set; }
    public ExternalIntegrationType ExternalIntegrationType { get; set; }
}