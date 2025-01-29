using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.Models.Users;

namespace RomanTourNotification.Application.Users;

public class UserService : IUserService
{
    public Task CreateAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByIdAsync(long userId)
    {
        throw new NotImplementedException();
    }
}