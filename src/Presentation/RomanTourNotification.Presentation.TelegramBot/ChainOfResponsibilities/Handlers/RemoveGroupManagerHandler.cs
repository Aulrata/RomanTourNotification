namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;

public class RemoveGroupManagerHandler : CommandHandler
{
    public override async Task Handle(HandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Iterator.CurrentWord != "remove_group_manager")
        {
            await base.Handle(context);
            return;
        }

        bool result = await context.HandlerServices.GroupService.RemoveGroupManager(
            context.Iterator.ObjectId,
            context.CancellationToken);

        // TODO Добавить логгер
        var backIterator = new Iterator($"groups choose_group show_group {context.Iterator.ObjectId}");

        HandlerContext backContext = context with { Iterator = backIterator };

        var groupHandler = new GroupHandler();
        await groupHandler.Handle(backContext);
    }
}