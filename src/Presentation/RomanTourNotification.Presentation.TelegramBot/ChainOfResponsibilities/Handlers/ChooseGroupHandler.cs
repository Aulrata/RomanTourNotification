using RomanTourNotification.Application.Models.Groups;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class ChooseGroupHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "choose_group")
        {
            await base.Handle(context);
            return;
        }

        if (context.Iterator.CountOfCommand > 2)
        {
            context.Iterator.MoveNext();

            var showGroupHandler = new ShowGroupHandler();

            await showGroupHandler.Handle(context);
        }
        else
        {
            IEnumerable<Group> groups = await context.GroupService.GetAllAsync(context.CancellationToken);

            var keyboardButtons = groups.Select(group => InlineKeyboardButton.WithCallbackData(
                    $"{group.Title}",
                    $"groups choose_group show_group {group.Id}"))
                .Select(button => (InlineKeyboardButton[])[button])
                .ToList();

            keyboardButtons.Add([InlineKeyboardButton.WithCallbackData("Назад", "groups")]);

            var keyboard = new InlineKeyboardMarkup(keyboardButtons);

            if (context.MessageId != 0)
            {
                await context.BotClient.EditMessageText(
                    chatId: context.User.ChatId,
                    messageId: context.MessageId,
                    text: "Выберите группу",
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
            }
            else
            {
                await context.BotClient.SendMessage(
                    chatId: context.User.ChatId,
                    text: "Выберите группу",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
            }
        }
    }
}