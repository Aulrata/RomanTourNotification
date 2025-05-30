using FluentMigrator;

namespace RomanTourNotification.Infrastructure.Persistence.Migrations;

[Migration(20250428_1)]
public class CreateExtraGroupTable : Migration
{
    public override void Up()
    {
        Execute.Sql("""
                    
                    ALTER TABLE groups
                    RENAME COLUMN group_id TO chat_id;

                    ALTER TABLE groups
                    ADD COLUMN manager_fullname VARCHAR(150);

                    CREATE TABLE extra_groups (
                        id BIGINT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
                        group_id BIGINT NOT NULL,
                        group_type group_type NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
                    );
                    
                    INSERT INTO extra_groups(group_id, group_type)
                    SELECT id, group_type
                    FROM groups;

                    ALTER TABLE groups
                    DROP COLUMN group_type;

                    """);
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE extra_groups;");
    }
}