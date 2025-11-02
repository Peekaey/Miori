using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Miori.Models;
using Miori.Models.Enums;
using NetCord;
using NetCord.Gateway;

namespace Miori.Helpers;

public class OauthHelpers : IOauthHelpers
{
    private readonly IConfiguration  _configuration;
    private readonly int _expirationMinutes = 10;

    public OauthHelpers(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    // https://docs.anilist.co/guide/auth/authorization-code
    public string GenerateAnilistAuthorisationUrl(ulong userDiscordId)
    {
        var clientId = _configuration["AnilistClientId"];
        var redirectUri = Uri.EscapeDataString(_configuration["AnilistRedirectUri"]);
        
        return "https://anilist.co/api/v2/oauth/authorize?" +
               "client_id=" + clientId +
               "&redirect_uri=" + redirectUri + 
               "&response_type=code" + 
               $"&state={GenerateStateParameter(userDiscordId, ExternalIntegrationType.Anilist)}";
    }
    // https://developer.spotify.com/documentation/web-api/tutorials/code-flow
    public string GenerateSpotifyAuthorisationUrl(ulong userDiscordId)
    {
        var clientId = _configuration["SpotifyClientId"];
        var redirectUri = Uri.EscapeDataString(_configuration["SpotifyRedirectUri"]);
        
        return "https://accounts.spotify.com/authorize?" +
               $"client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={redirectUri}" +
               $"&scope={Uri.EscapeDataString(_configuration["SpotifyScope"])}" +
               $"&state={GenerateStateParameter(userDiscordId, ExternalIntegrationType.Spotify)}";
    }

    public string GenerateStateParameter(ulong discordUserId, ExternalIntegrationType integrationType)
    {
        var payload = new OAuthState
        {
            DiscordUserId = discordUserId,
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Nonce = Guid.NewGuid().ToString("N"),
            ExternalIntegrationType = integrationType,
        };
        
        var json = JsonSerializer.Serialize(payload);
        var payloadBytes = Encoding.UTF8.GetBytes(json);
        
        // Create HMAC signature for signing
        // HMACSHA256 Signature is 32 bytes
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_configuration["StateSigningKey"])))
        {
            // Combine signature and payload then return
            var signatureBytes = hmac.ComputeHash(payloadBytes);
            var combined = payloadBytes.Concat(signatureBytes).ToArray();
            return Convert.ToBase64String(combined);
        }
        
    }
    
    public bool TryValidateState(string state, out ulong discordUserId)
    {
        discordUserId = 0;
        try
        {
            // First we destring from base64 - reverse from generation
            var combined = Convert.FromBase64String(state);
            // Subtract 32 bytes as that is the length of the HMACSHA256
            var jsonBytes = combined.Take(combined.Length - 32).ToArray();
            // Skip the length of the payload and take the last (signature)
            var signature = combined.Skip(combined.Length - 32).ToArray();
            
            // Verify signature by signing the signature bytes in same way
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_configuration["StateSigningKey"])))
            {
                var expectedSignature = hmac.ComputeHash(jsonBytes);
                if (!signature.SequenceEqual(expectedSignature))
                {
                    return false;
                }
            };
            
            // Get string from bytes
            var json = Encoding.UTF8.GetString(jsonBytes);
            // Finally we have the object
            var payload = JsonSerializer.Deserialize<OAuthState>(json);
            
            // Business logic is that we will expire the link if it is older than 10 minutes
            var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - payload.IssuedAt;
            // 10 minutes
            if (age > 600)
            {
                return false;
            }
            
            discordUserId = payload.DiscordUserId;
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}