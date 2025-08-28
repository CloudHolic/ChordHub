using ChordHub.Api.Core.Models;

namespace ChordHub.Api.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByDiscordIdAsync(string discordId);

    Task<User> CreateAsync(User user);

    Task<User> UpdateAsync(User user);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> ExistsByEmailAsync(string email);

    Task<bool> ExistsByDiscordIdAsync(string discordId);
}
