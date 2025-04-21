using RomanTourNotification.Application.Models.Users;

namespace RomanTourNotification.Application.Contracts.Users;

public interface IUserService
{
    public Task<long> CreateAsync(User user, CancellationToken cancellationToken);

    public Task<User?> GetByChatIdAsync(long userId, CancellationToken cancellationToken);

    public Task<IEnumerable<User>?> GetAllAsync(CancellationToken cancellationToken);

    public Task UpdateUserRoleAsync(long chatId, UserRole role, CancellationToken cancellationToken);
}