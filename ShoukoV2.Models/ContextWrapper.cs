using NetCord;

namespace ShoukoV2.Models;

public class ContextWrapper
{
    private readonly ApplicationCommandInteraction _commandInteraction;
    public ContextWrapper(ApplicationCommandInteraction contextInteraction,string commandName)
    {
        _commandInteraction = contextInteraction;
        CommandName = commandName;
        UserId = _commandInteraction.User.Id;
        GuildId = _commandInteraction.Guild?.Id ?? null;
        UserName = _commandInteraction.User.Username;
        InteractionId = Guid.NewGuid().ToString();
        InteractionTimeUtc = DateTime.UtcNow;
    }
    public string CommandName { get; }
    public ulong UserId { get;}
    public ulong? GuildId { get; }
    public string UserName { get; }
    public string InteractionId { get; }
    public DateTime InteractionTimeUtc { get; }
    
    
}