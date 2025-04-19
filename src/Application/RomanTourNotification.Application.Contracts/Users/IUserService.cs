using RomanTourNotification.Application.Models.Users;

namespace RomanTourNotification.Application.Contracts.Users;

public interface IUserService
{
    public Task<long> CreateAsync(User user, CancellationToken cancellationToken);

    public Task<User?> GetByIdAsync(long userId, CancellationToken cancellationToken);
}