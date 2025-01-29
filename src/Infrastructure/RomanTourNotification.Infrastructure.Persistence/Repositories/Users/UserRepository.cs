using Npgsql;
using RomanTourNotification.Application.Abstractions.Persistence.Repositories.Users;
using RomanTourNotification.Application.Models.Users;
using System.Data.Common;

namespace RomanTourNotification.Infrastructure.Persistence.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UserRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO users (first_name, last_name, user_role, chat_id, created_at)
                           VALUES (:first_name, :last_name, :user_role, :chat_id, :created_at)
                           RETURNING user_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("first_name", user.FirstName),
                new NpgsqlParameter("last_name", user.LastName),
                new NpgsqlParameter("user_role", user.Role),
                new NpgsqlParameter("chat_id", user.ChatId),
                new NpgsqlParameter("created_at", user.CreatedAt),
            },
        };

        object? generatedId = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(generatedId);
    }

    public Task<User?> GetUserByIdAsync(long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}