using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Contracts.Groups;

public interface IGroupService
{
    public Task<Group?> AddAsync(Group group, CancellationToken cancellationToken);

    public Task<long> DeleteAsync(long chatId, CancellationToken cancellationToken);

    public Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<Group>> GetAllWorksGroupsAsync(CancellationToken cancellationToken);

    public Task<Group?> GetByIdAsync(long id, CancellationToken cancellationToken);

    public Task<IEnumerable<GroupType>> GetAllGroupTypesByIdAsync(long groupId, CancellationToken cancellationToken);

    public Task<bool> AddGroupTypeByIdAsync(long groupId, GroupType groupType, CancellationToken cancellationToken);

    public Task<bool> RemoveGroupTypeByIdAsync(long groupId, GroupType groupType, CancellationToken cancellationToken);

    public Task<bool> AddGroupManager(long groupId, string managerFullname, CancellationToken cancellationToken);

    public Task<bool> RemoveGroupManager(long groupId, CancellationToken cancellationToken);
}