using RomanTourNotification.Application.Models.Gateway;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class AddGroupManagerHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "add_group_manager")
        {
            await base.Handle(context);
            return;
        }

        if (context.Iterator.CountOfCommand > 5)
        {
            context.Iterator.MoveNext();
            long employeeId = long.Parse(context.Iterator.CurrentWord);

            Employee employee =
                (await context.HandlerServices.LoadEmployees.GetAllEmployeesAsync(context.CancellationToken))
                .First(e => e.EmployeeId == employeeId);

            bool result = await context.HandlerServices.GroupService.AddGroupManager(
                context.Iterator.ObjectId,
                employee.GetEmployeeFullName(),
                context.CancellationToken);

            // TODO Добавить логгер
            var backIterator = new Iterator($"groups choose_group show_group {context.Iterator.ObjectId}");

            HandlerContext backContext = context with { Iterator = backIterator };

            var groupHandler = new GroupHandler();
            await groupHandler.Handle(backContext);
        }
        else
        {
            IEnumerable<Employee> allEmployees =
                await context.HandlerServices.LoadEmployees.GetAllEmployeesAsync(context.CancellationToken);

            const int buttonsPerRow = 1;
            var keyboardButtons = new List<InlineKeyboardButton[]>();
            var tempButtons = new List<InlineKeyboardButton>();

            foreach (Employee? employee in allEmployees)
            {
                string callbackData = $"groups choose_group show_group {context.Iterator.ObjectId} add_group_manager {employee.EmployeeId}";
                int byteLength = System.Text.Encoding.UTF8.GetByteCount(callbackData);
                if (byteLength > 64)
                {
                    Console.WriteLine($"Предупреждение: callback_data слишком длинная ({byteLength} байт) для сотрудника {employee.EmployeeId}");
                    continue; // Пропускаем проблемные кнопки
                }

                var button = InlineKeyboardButton.WithCallbackData(
                    $"{employee.GetEmployeeFullName()}",
                    callbackData);

                tempButtons.Add(button);

                if (tempButtons.Count != buttonsPerRow) continue;

                keyboardButtons.Add(tempButtons.ToArray());
                tempButtons.Clear();
            }

            if (tempButtons.Count > 0)
                keyboardButtons.Add(tempButtons.ToArray());

            var keyboard = new InlineKeyboardMarkup(keyboardButtons);

            if (context.MessageId != 0)
            {
                await context.BotClient.EditMessageText(
                    chatId: context.User.ChatId,
                    messageId: context.MessageId,
                    text: "Выберите менеджера, которого хотите добавить",
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
            }
            else
            {
                await context.BotClient.SendMessage(
                    context.User.ChatId,
                    "Выберите пункт настроек",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
            }
        }
    }
}