using RomanTourNotification.Application.Models.Extensions;
using RomanTourNotification.Application.Models.Groups;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class RemoveGroupTypeHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "remove_group_type")
        {
            await base.Handle(context);
            return;
        }

        long groupId = context.Iterator.ObjectId;
        var group = (await context.HandlerServices.GroupService.GetAllGroupTypesByIdAsync(groupId, context.CancellationToken)).ToList();

        List<InlineKeyboardButton> buttons = [];

        InlineKeyboardMarkup keyboard;
        if (group.Count != 0)
        {
            buttons.AddRange(group.Select(groupType => InlineKeyboardButton.WithCallbackData(
            $"{groupType.GetDescription()}",
            $"groups choose_group show_group {context.Iterator.ObjectId} remove_group_type {(int)groupType}")));

            keyboard = new InlineKeyboardMarkup(
            [
                buttons,
                [
                    InlineKeyboardButton.WithCallbackData(
                        "Назад",
                        $"groups choose_group show_group {context.Iterator.ObjectId}"),
                ]
            ]);
        }
        else
        {
            keyboard = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        "У группы нет типов (назад)",
                        $"groups choose_group show_group {context.Iterator.ObjectId}"),
                ]
            ]);
        }

        if (context.Iterator.CountOfCommand > 5)
        {
            context.Iterator.MoveNext();

            await context.HandlerServices.GroupService.RemoveGroupTypeByIdAsync(
                context.Iterator.ObjectId,
                (GroupType)int.Parse(context.Iterator.CurrentWord),
                context.CancellationToken);

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
                    text: "Выберите тип, который хотите удалить",
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