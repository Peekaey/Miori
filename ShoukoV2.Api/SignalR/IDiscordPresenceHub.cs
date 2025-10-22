using ShoukoV2.Models;

namespace ShoukoV2.Api.SignalR;

public interface IDiscordPresenceHub
{
    Task SendMessage(DiscordRichPresenceSocketDto message);
    Task Subscribe();
    Task Unsubscribe();
}