using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Abstractions.Persistence.Repositories.Groups;

public interface IGroupRepository
{
    public Task<long> CreateAsync(Group group, CancellationToken cancellationToken);

    public Task<long> DeleteAsync(long groupId, CancellationToken cancellationToken);

    public Task<IEnumerable<Group>?> GetAllAsync(CancellationToken cancellationToken);

    public Task<Group?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken);
}