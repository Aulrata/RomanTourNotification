using RomanTourNotification.Application.Abstractions.Persistence.Repositories.Users;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.Models.Users;
using System.Transactions;

namespace RomanTourNotification.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<string> CreateAsync(User user, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        User? oldUser = await _userRepository.GetUserByChatIdAsync(user.ChatId, cancellationToken);

        Console.WriteLine("User created");

        if (oldUser is not null)
            return "User already exists.";

        long userId = await _userRepository.CreateUserAsync(user, cancellationToken);

        transaction.Complete();

        return "User created successfully.";
    }

    public async Task<User?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetUserByChatIdAsync(userId, cancellationToken);
        return user;
    }
}