namespace RomanTourNotification.Application.Contracts.ReturnNotification;

public interface IReturnNotificationService
{
    public Task<string> GetReturnMessageAsync(string manager, CancellationToken cancellationToken);

    public Task<string> GetReturnCompleteMessageAsync(CancellationToken cancellationToken);
}