namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public abstract class CommandHandler : ICommandHandler
{
    private ICommandHandler? _nextHandler;

    public ICommandHandler SetNext(ICommandHandler handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public virtual Task Handle(HandlerContext context)
    {
        return _nextHandler?.Handle(context) ?? Task.CompletedTask;
    }
}