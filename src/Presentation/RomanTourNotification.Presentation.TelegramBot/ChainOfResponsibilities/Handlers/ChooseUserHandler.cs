using RomanTourNotification.Application.Models.Users;
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

        if (context.Iterator.CountOfCommand > 2)
        {
            context.Iterator.MoveNext();

            var showUserHandler = new ShowUserHandler();

            await showUserHandler.Handle(context);
        }
        else
        {
            IAsyncEnumerable<User> users = context.UserService.GetAllAsync(context.CancellationToken);

            var keyboardButtons = new List<InlineKeyboardButton[]>();
            await foreach (User? user in users.WithCancellation(context.CancellationToken))
            {
                var button = InlineKeyboardButton.WithCallbackData(
                    $"{user.FirstName} {user.LastName}",
                    $"users choose_user show_user {user.ChatId}");
                keyboardButtons.Add([button]);
            }

            keyboardButtons.Add([InlineKeyboardButton.WithCallbackData("Назад", "users")]);

            var keyboard = new InlineKeyboardMarkup(keyboardButtons);

            if (context.MessageId != 0)
            {
                if (context.User.Id is null)
                    return;

                await context.BotClient.EditMessageText(
                    chatId: context.User.ChatId,
                    messageId: context.MessageId,
                    text: "Выберите пользователя",
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
            }
            else
            {
                await context.BotClient.SendMessage(
                    chatId: context.User.ChatId,
                    text: "Выберите пользователя",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
            }
        }
    }
}