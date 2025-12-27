using RomanTourNotification.Application.Models.Extensions;
using RomanTourNotification.Application.Models.Groups;
using RomanTourNotification.Application.Models.Users;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class ShowGroupHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "show_group")
        {
            await base.Handle(context);
            return;
        }

        context.Iterator.MoveNext();

        long groupId = long.Parse(context.Iterator.CurrentWord);

        context.Iterator.ObjectId = groupId;

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData(
                    "Добавить тип",
                    $"groups choose_group show_group {groupId} add_group_type"),
                InlineKeyboardButton.WithCallbackData(
                    "Удалить тип",
                    $"groups choose_group show_group {groupId} remove_group_type"),
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData(
                    "Добавить менеджера",
                    $"groups choose_group show_group {groupId} add_group_manager"),
                InlineKeyboardButton.WithCallbackData(
                    "Удалить менеджера",
                    $"groups choose_group show_group {groupId} remove_group_manager"),
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("Назад", "groups choose_group"),
            },
        };

        if (context.User.Role == UserRole.Developer)
        {
            var developerButtons = InlineKeyboardButton.WithCallbackData(
                "Отправить уведомление",
                $"groups choose_group show_group {groupId} send_notification");

            buttons.Add([developerButtons]);
        }

        var keyboard = new InlineKeyboardMarkup(buttons);

        if (context.Iterator.CountOfCommand > 4)
        {
            context.Iterator.MoveNext();

            var addGroupTypeHandler = new AddGroupTypeHandler();
            var removeGroupTypeHandler = new RemoveGroupTypeHandler();
            var addGroupManagerHandler = new AddGroupManagerHandler();
            var removeGroupManagerHandler = new RemoveGroupManagerHandler();
            var sendNotificationHandler = new SendNotificationHandler();
            addGroupTypeHandler
                .SetNext(removeGroupTypeHandler)
                .SetNext(addGroupManagerHandler)
                .SetNext(removeGroupManagerHandler)
                .SetNext(sendNotificationHandler);
            await addGroupTypeHandler.Handle(context);
        }
        else
        {
            Group? group = await context.HandlerServices.GroupService.GetByIdAsync(groupId, context.CancellationToken);

            if (group is null)
                return;

            var types = (await context.HandlerServices.GroupService.GetAllGroupTypesByIdAsync(group.Id, context.CancellationToken)).ToList();

            var sb = new StringBuilder();
            foreach (GroupType type in types) sb.Append($"{type.GetDescription()}, ");
            string groupTypes = string.IsNullOrEmpty(sb.ToString()) ? "'Отсутствуют'" : sb.ToString().Remove(sb.ToString().Length - 2);
            string manager = string.IsNullOrEmpty(group.ManagerFullname) ? "'Отсутствует'" : group.ManagerFullname;

            string text =
                $"Информация о группе\n" +
                $"  Название: {group.Title}\n" +
                $"  Менеджер: {manager}\n" +
                $"  Типы группы: {groupTypes}";

            if (context.MessageId != 0)
            {
                await context.BotClient.EditMessageText(
                    chatId: context.User.ChatId,
                    messageId: context.MessageId,
                    text: text,
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
            }
            else
            {
                await context.BotClient.SendMessage(
                    chatId: context.User.ChatId,
                    text: text,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
            }
        }
    }
}