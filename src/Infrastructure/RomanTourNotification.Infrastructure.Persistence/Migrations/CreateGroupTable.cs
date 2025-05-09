using FluentMigrator;

namespace RomanTourNotification.Infrastructure.Persistence.Migrations;

[Migration(20250203_1)]
public class CreateGroupTable : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TYPE group_type AS ENUM ('unspecified', 'payment', 'arrival')
        ");

        Execute.Sql(@"
            CREATE TABLE groups (
                id BIGINT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
                title VARCHAR(50) NOT NULL,
                group_id BIGINT NOT NULL,
                user_id BIGINT NOT NULL,
                group_type group_type NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
            );
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            DROP TABLE groups;
            DROP TYPE group_type;
        ");
    }
}