using RomanTourNotification.Application.Models.Users;

namespace RomanTourNotification.Application.Abstractions.Persistence.Repositories.Users;

public interface IUserRepository
{
    public Task<long> CreateUserAsync(User user, CancellationToken cancellationToken);

    public Task<User?> GetUserByIdAsync(long userId, CancellationToken cancellationToken);
}