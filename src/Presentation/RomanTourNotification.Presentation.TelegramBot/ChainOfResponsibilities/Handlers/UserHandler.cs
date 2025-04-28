using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class UserHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "users")
        {
            await base.Handle(context);
            return;
        }

        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("Добавить пользователя", "users add_user"),
                InlineKeyboardButton.WithCallbackData("Выбрать пользователя", "users choose_user")
            ],
            [
                InlineKeyboardButton.WithCallbackData("Назад", "/start")
            ]
        ]);

        if (context.Iterator.CountOfCommand > 1)
        {
            context.Iterator.MoveNext();

            var userAddHandler = new UserAddHandler();
            var chooseUserHandler = new ChooseUserHandler();
            await userAddHandler.SetNext(chooseUserHandler);
            await userAddHandler.Handle(context);
        }
        else
        {
            if (context.MessageId != 0)
            {
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
}