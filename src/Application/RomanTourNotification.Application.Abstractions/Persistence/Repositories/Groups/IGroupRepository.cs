using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Abstractions.Persistence.Repositories.Groups;

public interface IGroupRepository
{
    public Task<long> CreateAsync(Group group, CancellationToken cancellationToken);

    public Task<long> DeleteByChatIdAsync(long chatId, CancellationToken cancellationToken);

    public Task<IEnumerable<Group>> GetAllWorksGroupsAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<GroupType>> GetAllGroupTypesByIdAsync(long groupId, CancellationToken cancellationToken);

    public Task<Group?> GetByIdAsync(long id, CancellationToken cancellationToken);

    public Task AddGroupTypeByIdAsync(long groupId, GroupType groupType, CancellationToken cancellationToken);

    public Task RemoveGroupTypeByIdAsync(long groupId, GroupType groupType, CancellationToken cancellationToken);

    public Task AddManagerByIdAsync(long groupId, string managerFullname, CancellationToken cancellationToken);

    public Task RemoveManagerByIdAsync(long groupId, CancellationToken cancellationToken);
}