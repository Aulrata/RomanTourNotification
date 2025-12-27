namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public interface ICommandHandler
{
    public ICommandHandler SetNext(ICommandHandler handler);

    public Task Handle(HandlerContext context);
}