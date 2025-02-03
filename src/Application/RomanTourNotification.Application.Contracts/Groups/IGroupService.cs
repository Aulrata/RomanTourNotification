using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Contracts.Groups;

public interface IGroupService
{
    public Task<string> AddGroupAsync(Group group, CancellationToken cancellationToken);

    public Task<IEnumerable<Group>?> GetAllGroupsAsync(CancellationToken cancellationToken);
}