namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public interface ICommandHandler
{
    public Task<ICommandHandler> SetNext(ICommandHandler handler);

    public Task Handle(HandlerContext context);
}