using FluentMigrator;

namespace RomanTourNotification.Infrastructure.Persistence.Migrations;

/// <summary>
/// Step 1 of 2: adds new enum values to group_type.
///
/// PostgreSQL does not allow using a freshly-added enum value in the same
/// transaction where ALTER TYPE ... ADD VALUE was executed (error 55P04).
/// Therefore this migration runs outside any transaction so the new values
/// are committed immediately and become available to the next migration.
/// </summary>
[Migration(20260517_1, TransactionBehavior.None)]
public class SplitArrivalGroupTypeAddValues : Migration
{
    public override void Up()
    {
        Execute.Sql("ALTER TYPE group_type ADD VALUE IF NOT EXISTS 'documents_for_departure';");
        Execute.Sql("ALTER TYPE group_type ADD VALUE IF NOT EXISTS 'air_tickets';");
    }

    public override void Down()
    {
        // PostgreSQL does not support removing enum values.
    }
}
