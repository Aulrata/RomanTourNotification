using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Abstractions.Persistence.Repositories.Users;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.Models.Users;
using System.Transactions;

namespace RomanTourNotification.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<long> CreateAsync(User user, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        User? oldUser = await _userRepository.GetUserByChatIdAsync(user.ChatId, cancellationToken);

        // TODO Доабвить логгер "User already exists.";
        if (oldUser is not null)
            return oldUser.Id;

        long userId = await _userRepository.CreateUserAsync(user, cancellationToken);

        _logger.LogInformation("User created successfully.");

        transaction.Complete();

        return userId;
    }

    public async Task<User?> GetByChatIdAsync(long userId, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetUserByChatIdAsync(userId, cancellationToken);
        return user;
    }

    public IAsyncEnumerable<User> GetAllAsync(CancellationToken cancellationToken)
    {
        return _userRepository.GetAllUsersAsync(cancellationToken);
    }

    public async Task UpdateUserRoleAsync(long chatId, UserRole role, CancellationToken cancellationToken)
    {
        await _userRepository.UpdateUserRoleAsync(chatId, role, cancellationToken);
    }
}