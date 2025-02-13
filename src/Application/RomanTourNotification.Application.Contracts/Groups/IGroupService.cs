using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Contracts.Groups;

public interface IGroupService
{
    public Task<Group?> AddAsync(Group group, CancellationToken cancellationToken);

    public Task<long> DeleteAsync(long groupId, CancellationToken cancellationToken);

    public Task<IEnumerable<Group>?> GetAllAsync(CancellationToken cancellationToken);
}