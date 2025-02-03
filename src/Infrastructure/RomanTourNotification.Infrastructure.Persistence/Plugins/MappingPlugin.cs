using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Npgsql;
using RomanTourNotification.Application.Models.Groups;
using RomanTourNotification.Application.Models.Users;

namespace RomanTourNotification.Infrastructure.Persistence.Plugins;

/// <summary>
///     Plugin for configuring NpgsqlDataSource's mappings
///     ie: enums, composite types
/// </summary>
public class MappingPlugin : IPostgresDataSourcePlugin
{
    public void Configure(NpgsqlDataSourceBuilder dataSource)
    {
        dataSource.MapEnum<UserRole>("user_role");
        dataSource.MapEnum<GroupType>("group_type");
    }
}