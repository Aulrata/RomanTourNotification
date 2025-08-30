using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
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
                           INSERT INTO groups (title, chat_id, user_id, created_at)
                           VALUES (:title, :chat_id, :user_id, :created_at)
                           RETURNING id;
                           """;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("title", group.Title),
                new NpgsqlParameter("chat_id", group.ChatId),
                new NpgsqlParameter("user_id", group.UserId),
                new NpgsqlParameter("created_at", group.CreatedAt),
            },
        };

        object? generatedId = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(generatedId);
    }

    public async Task<long> DeleteByChatIdAsync(long chatId, CancellationToken cancellationToken)
    {
        const string sql = """
                           DELETE FROM groups
                           WHERE chat_id = :chat_id
                           RETURNING chat_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("chat_id", chatId),
            },
        };

        object? deletedId = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt64(deletedId);
    }

    public async Task<IEnumerable<Group>> GetAllWorksGroupsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT g.id as id,
                                  g.title as title,
                                  g.chat_id as chat_id,
                                  g.user_id as user_id,
                                  g.manager_fullname as manager_fullname,
                                  eg.group_type as group_type,
                                  g.created_at as created_at
                           FROM groups g
                           JOIN extra_groups eg ON g.id = eg.group_id
                           WHERE eg.group_type != 'unspecified';
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var groups = new List<Group>();
        while (await reader.ReadAsync(cancellationToken))
        {
            string? managerFullname = reader.GetNullableString(reader.GetOrdinal("manager_fullname"));
            var group = new Group(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("title")),
                reader.GetInt64(reader.GetOrdinal("chat_id")),
                reader.GetInt64(reader.GetOrdinal("user_id")),
                string.IsNullOrEmpty(managerFullname) ? string.Empty : managerFullname,
                reader.GetFieldValue<GroupType>(reader.GetOrdinal("group_type")),
                reader.GetDateTime(reader.GetOrdinal("created_at")));

            groups.Add(group);
        }

        return groups;
    }

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT id, title, chat_id, user_id, manager_fullname, created_at
                           FROM groups;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var groups = new List<Group>();
        while (await reader.ReadAsync(cancellationToken))
        {
            string? managerFullname = reader.GetNullableString(reader.GetOrdinal("manager_fullname"));

            var group = new Group(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("title")),
                reader.GetInt64(reader.GetOrdinal("chat_id")),
                reader.GetInt64(reader.GetOrdinal("user_id")),
                string.IsNullOrEmpty(managerFullname) ? string.Empty : managerFullname,
                GroupType.Unspecified,
                reader.GetDateTime(reader.GetOrdinal("created_at")));

            groups.Add(group);
        }

        return groups;
    }

    public async Task<IEnumerable<GroupType>> GetAllGroupTypesByIdAsync(
        long groupId,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT group_type
                           FROM extra_groups
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

        var groupTypes = new List<GroupType>();
        while (await reader.ReadAsync(cancellationToken))
        {
            groupTypes.Add(reader.GetFieldValue<GroupType>(reader.GetOrdinal("group_type")));
        }

        return groupTypes;
    }

    public async Task<Group?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT id, title, chat_id, user_id, manager_fullname, created_at
                           FROM groups 
                           WHERE chat_id = :chat_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("chat_id", chatId),
            },
        };

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            string? managerFullname = reader.GetNullableString(reader.GetOrdinal("manager_fullname"));

            return new Group(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("title")),
                reader.GetInt64(reader.GetOrdinal("chat_id")),
                reader.GetInt64(reader.GetOrdinal("user_id")),
                string.IsNullOrEmpty(managerFullname) ? string.Empty : managerFullname,
                GroupType.Unspecified,
                reader.GetDateTime(reader.GetOrdinal("created_at")));
        }

        return null;
    }

    public async Task AddGroupTypeByIdAsync(long groupId, GroupType groupType, CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO extra_groups (group_id, group_type)
                           VALUES (:group_id, :group_type);
                           """;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("group_id", groupId),
                new NpgsqlParameter("group_type", groupType),
            },
        };

        await command.ExecuteReaderAsync(cancellationToken);
    }

    public async Task RemoveGroupTypeByIdAsync(long groupId, GroupType groupType, CancellationToken cancellationToken)
    {
        const string sql = """
                           DELETE FROM extra_groups
                           WHERE group_id = :group_id and group_type = :group_type;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("group_id", groupId),
                new NpgsqlParameter("group_type", groupType),
            },
        };

        await command.ExecuteReaderAsync(cancellationToken);
    }

    public async Task AddManagerByIdAsync(long groupId, string managerFullname, CancellationToken cancellationToken)
    {
        const string sql = """
                           UPDATE groups
                           SET manager_fullname = :manager_fullname
                           WHERE id = :group_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("group_id", groupId),
                new NpgsqlParameter("manager_fullname", managerFullname),
            },
        };

        await command.ExecuteReaderAsync(cancellationToken);
    }

    public async Task RemoveManagerByIdAsync(long groupId, CancellationToken cancellationToken)
    {
        const string sql = """
                           UPDATE groups
                           SET manager_fullname = null
                           WHERE id = :group_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("group_id", groupId),
            },
        };

        await command.ExecuteReaderAsync(cancellationToken);
    }
}