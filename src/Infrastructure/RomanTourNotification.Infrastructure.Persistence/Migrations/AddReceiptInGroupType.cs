using FluentMigrator;

namespace RomanTourNotification.Infrastructure.Persistence.Migrations;

[Migration(20260209_1)]
public class AddReceiptInGroupType : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            ALTER TYPE group_type ADD VALUE 'receipt';
        ");
    }

    public override void Down()
    {
        // К сожалению, PostgreSQL не поддерживает удаление значений из enum
        // Поэтому откат этой миграции требует пересоздания типа
        Execute.Sql(@"
            -- Создаем временный тип с старыми значениями
            CREATE TYPE group_type_old AS ENUM ('unspecified', 'payment', 'arrival', 'return');
            
            -- Изменяем колонку на временный тип
            ALTER TABLE groups 
                ALTER COLUMN group_type TYPE group_type_old 
                USING group_type::text::group_type_old;
            
            -- Удаляем новый тип
            DROP TYPE group_type;
            
            -- Переименовываем временный тип обратно
            ALTER TYPE group_type_old RENAME TO group_type;
        ");
    }
}