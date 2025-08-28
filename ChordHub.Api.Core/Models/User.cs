namespace ChordHub.Api.Core.Models;

public class User
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    
    public string? DiscordId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    public void LinkDiscord(string discordId)
    {
        if (string.IsNullOrWhiteSpace(discordId))
            throw new ArgumentException("Discord ID is needed.");
        
        DiscordId = discordId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display Name is needed.");

        DisplayName = displayName;
        UpdatedAt = DateTime.UtcNow;
    }
}
