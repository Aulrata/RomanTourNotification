namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public interface ICommandHandler
{
    Task<ICommandHandler> SetNext(ICommandHandler handler);

    Task Handle(HandlerContext context);
}