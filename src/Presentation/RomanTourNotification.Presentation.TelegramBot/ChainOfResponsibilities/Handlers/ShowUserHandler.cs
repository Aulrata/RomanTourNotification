using RomanTourNotification.Application.Extensions;
using RomanTourNotification.Application.Models.Users;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class ShowUserHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "show_user")
        {
            await base.Handle(context);
            return;
        }

        context.Iterator.MoveNext();

        int chatId = int.Parse(context.Iterator.CurrentWord);

        context.Iterator.ObjectId = chatId;

        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData(
                    "Изменить роль",
                    $"users choose_user show_user {chatId} choose_role"),
                InlineKeyboardButton.WithCallbackData("Назад", "users choose_user")
            ]
        ]);

        if (context.Iterator.CountOfCommand > 4)
        {
            context.Iterator.MoveNext();

            var chooseRoleHandler = new ChooseRoleHandler();
            await chooseRoleHandler.Handle(context);
        }
        else
        {
            User? user = await context.HandlerServices.UserService.GetByChatIdAsync(chatId, context.CancellationToken);

            if (user is null)
                return;

            string text =
                $"Информация о пользователе\n" +
                $"  Имя: {user.FirstName}\n" +
                $"  Фамилия: {(string.IsNullOrEmpty(user.LastName) ? "'Отсутствует'" : user.LastName)}\n" +
                $"  Роль: {user.Role.GetDescription()}";

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