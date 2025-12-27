using RomanTourNotification.Application.Models.Extensions;
using RomanTourNotification.Application.Models.Groups;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class SendNotificationHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "send_notification")
        {
            await base.Handle(context);
            return;
        }

        IEnumerable<GroupType> groupTypes = await context.HandlerServices.GroupService.GetAllGroupTypesByIdAsync(
            context.Iterator.ObjectId,
            context.CancellationToken);

        IEnumerable<InlineKeyboardButton> buttons = groupTypes
            .Select(groupType => InlineKeyboardButton.WithCallbackData(
                $"{groupType.GetDescription()}",
                $"groups choose_group show_group {context.Iterator.ObjectId} send_notification {(int)groupType}"));

        var keyboard = new InlineKeyboardMarkup([
            buttons,
            [
                InlineKeyboardButton.WithCallbackData(
                    "Назад",
                    $"groups choose_group show_group {context.Iterator.ObjectId}"),
            ]
        ]);

        if (context.Iterator.CountOfCommand > 5)
        {
            context.Iterator.MoveNext();

            var groupType = (GroupType)int.Parse(context.Iterator.CurrentWord);
            long groupId = context.Iterator.ObjectId;

            Group? group = await context.HandlerServices.GroupService.GetByIdAsync(groupId, context.CancellationToken);

            await context.HandlerServices.NotificationService.SendForcedNotificationAsync(group, groupType, context.CancellationToken);

            var backIterator = new Iterator($"groups choose_group show_group {context.Iterator.ObjectId}");

            HandlerContext backContext = context with { Iterator = backIterator };

            var groupHandler = new GroupHandler();
            await groupHandler.Handle(backContext);
        }
        else
        {
            if (context.MessageId != 0)
            {
                await context.BotClient.EditMessageText(
                    chatId: context.User.ChatId,
                    messageId: context.MessageId,
                    text: "Выберите тип, по которому хотите отправить уведомление",
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