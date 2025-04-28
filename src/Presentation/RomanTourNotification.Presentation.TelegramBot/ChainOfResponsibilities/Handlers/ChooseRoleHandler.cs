using RomanTourNotification.Application.Models.Users;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class ChooseRoleHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "choose_role")
        {
            await base.Handle(context);
            return;
        }

        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData(
                    "Администратор",
                    $"users choose_user show_user {context.Iterator.ObjectId} choose_role {(int)UserRole.Admin}"),
                InlineKeyboardButton.WithCallbackData(
                    "Менеджер",
                    $"users choose_user show_user {context.Iterator.ObjectId} choose_role {(int)UserRole.Manager}"),
            ],
            [
                InlineKeyboardButton.WithCallbackData(
                    "Не указана",
                    $"users choose_user show_user {context.Iterator.ObjectId} choose_role {(int)UserRole.Unspecified}"),
                InlineKeyboardButton.WithCallbackData(
                    "Разработчик",
                    $"users choose_user show_user {context.Iterator.ObjectId} choose_role {(int)UserRole.Developer}"),
            ],
            [
                InlineKeyboardButton.WithCallbackData(
                    "Назад",
                    $"users choose_user show_user {context.Iterator.ObjectId}"),
            ]
        ]);

        if (context.Iterator.CountOfCommand > 5)
        {
            context.Iterator.MoveNext();

            await context.UserService.UpdateUserRoleAsync(
                context.Iterator.ObjectId,
                (UserRole)int.Parse(context.Iterator.CurrentWord),
                context.CancellationToken);

            var backIterator = new Iterator($"users choose_user show_user {context.Iterator.ObjectId}");

            HandlerContext backContext = context with { Iterator = backIterator };

            var userHandler = new UserHandler();
            await userHandler.Handle(backContext);
        }
        else
        {
            if (context.MessageId != 0)
            {
                await context.BotClient.EditMessageText(
                    chatId: context.User.ChatId,
                    messageId: context.MessageId,
                    text: "Выберите роль",
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