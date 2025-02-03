namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public abstract class CommandHandler : ICommandHandler
{
    private ICommandHandler? _nextHandler;

    public Task<ICommandHandler> SetNext(ICommandHandler handler)
    {
        _nextHandler = handler;
        return Task.FromResult(handler);
    }

    public virtual Task Handle(HandlerContext context)
    {
        return _nextHandler?.Handle(context) ?? Task.CompletedTask;
    }
}