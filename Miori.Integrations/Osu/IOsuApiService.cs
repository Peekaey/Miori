using Miori.Models;
using Miori.Models.Osu;

namespace Miori.Integrations.Osu;

public interface IOsuApiService
{
    Task<Result<string>> GetOsuProfileIdForNewRegister(OsuTokenResponse tokenResponse);
    Task<OsuProfileDto> GetOsuUserData(ulong discordUserId);
}