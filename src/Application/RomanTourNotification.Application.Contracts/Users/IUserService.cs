using RomanTourNotification.Application.Models.Users;

namespace RomanTourNotification.Application.Contracts.Users;

public interface IUserService
{
    public Task CreateAsync(User user);

    public Task<User?> GetByIdAsync(long userId);
}