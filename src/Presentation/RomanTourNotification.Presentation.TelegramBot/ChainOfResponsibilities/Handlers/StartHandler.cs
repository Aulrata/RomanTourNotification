using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class StartHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Message != "/start")
        {
            await base.Handle(context);
            return;
        }

        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("Пользователи", "users"),
                InlineKeyboardButton.WithCallbackData("Обо мне", "about_me")
            ],
            [
                InlineKeyboardButton.WithCallbackData("Группы", "groups"),
                InlineKeyboardButton.WithCallbackData("Тест", "nothing")
            ]
        ]);

        await context.BotClient.SendMessage(
            context.User.ChatId,
            "Выберите пункт настроек",
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard);
    }
}