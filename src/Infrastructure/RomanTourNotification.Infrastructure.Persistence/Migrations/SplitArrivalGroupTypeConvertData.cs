using FluentMigrator;

namespace RomanTourNotification.Infrastructure.Persistence.Migrations;

/// <summary>
/// Step 2 of 2: converts existing 'arrival' rows to 'documents_for_departure'.
///
/// Runs after <see cref="SplitArrivalGroupTypeAddValues"/> has committed the
/// new enum values, so they are safe to reference here inside a transaction.
/// </summary>
[Migration(20260517_2)]
public class SplitArrivalGroupTypeConvertData : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            UPDATE extra_groups
            SET group_type = 'documents_for_departure'
            WHERE group_type = 'arrival';
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            UPDATE extra_groups
            SET group_type = 'arrival'
            WHERE group_type = 'documents_for_departure';
        ");
    }
}
