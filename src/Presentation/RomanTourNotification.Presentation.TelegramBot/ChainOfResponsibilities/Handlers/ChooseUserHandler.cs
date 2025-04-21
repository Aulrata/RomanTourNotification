using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class ChooseUserHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "choose_user")
        {
            await base.Handle(context);
            return;
        }

        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("Назад", "users"),
            ]
        ]);

        if (context.MessageId != 0)
        {
            if (context.User.Id is null)
                return;

            await context.BotClient.EditMessageReplyMarkup(
                chatId: context.User.ChatId,
                messageId: context.MessageId,
                keyboard,
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