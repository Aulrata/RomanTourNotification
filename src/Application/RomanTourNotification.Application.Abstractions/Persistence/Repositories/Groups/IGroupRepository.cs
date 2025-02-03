using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Abstractions.Persistence.Repositories.Groups;

public interface IGroupRepository
{
    public Task<long> CreateAsync(Group group, CancellationToken cancellationToken);

    public Task<IEnumerable<Group>?> GetAllAsync(CancellationToken cancellationToken);

    public Task<Group?> GetByGroupIdAsync(long groupId, CancellationToken cancellationToken);
}