using Telegram.Bot;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class UserAddHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Message != "user_add")
        {
            await base.Handle(context);
            return;
        }

        await context.BotClient.SendMessage(
            context.User.ChatId,
            "Выберите пункт настроек",
            cancellationToken: context.CancellationToken);
    }
}