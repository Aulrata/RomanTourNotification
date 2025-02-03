using Npgsql;
using RomanTourNotification.Application.Abstractions.Persistence.Repositories.Groups;
using RomanTourNotification.Application.Models.Groups;
using System.Data.Common;

namespace RomanTourNotification.Infrastructure.Persistence.Repositories.Groups;

public class GroupRepository : IGroupRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public GroupRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> CreateAsync(Group group, CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO groups (title, group_id, user_id, approve, group_type, created_at)
                           VALUES (:title, :group_id, :user_id, :approve, :group_type, :created_at)
                           RETURNING id;
                           """;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("title", group.Title),
                new NpgsqlParameter("group_id", group.GroupId),
                new NpgsqlParameter("user_id", group.UserId),
                new NpgsqlParameter("approve", group.Approve),
                new NpgsqlParameter("group_type", group.GroupType),
                new NpgsqlParameter("created_at", group.CreatedAt),
            },
        };

        object? generatedId = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(generatedId);
    }

    public async Task<IEnumerable<Group>?> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT id, title, group_id, user_id, approve, group_type, created_at
                           FROM groups
                           WHERE approve IS NOT 'unspecified';
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var groups = new List<Group>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var group = new Group(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("title")),
                reader.GetInt64(reader.GetOrdinal("group_id")),
                reader.GetInt64(reader.GetOrdinal("user_id")),
                reader.GetBoolean(reader.GetOrdinal("approve")),
                reader.GetFieldValue<GroupType>(reader.GetOrdinal("group_type")),
                reader.GetDateTime(reader.GetOrdinal("created_at")));

            groups.Add(group);
        }

        return groups;
    }

    public async Task<Group?> GetByGroupIdAsync(long groupId, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT id, title, group_id, user_id, approve, group_type, created_at
                           FROM groups
                           WHERE group_id = :group_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("group_id", groupId),
            },
        };

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new Group(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("title")),
                reader.GetInt64(reader.GetOrdinal("group_id")),
                reader.GetInt64(reader.GetOrdinal("user_id")),
                reader.GetBoolean(reader.GetOrdinal("approve")),
                reader.GetFieldValue<GroupType>(reader.GetOrdinal("group_type")),
                reader.GetDateTime(reader.GetOrdinal("created_at")));
        }

        return null;
    }
}