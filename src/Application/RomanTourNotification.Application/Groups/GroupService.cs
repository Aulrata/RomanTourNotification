using RomanTourNotification.Application.Abstractions.Persistence.Repositories.Groups;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Models.Groups;
using System.Transactions;

namespace RomanTourNotification.Application.Groups;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;

    public GroupService(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<string> AddGroupAsync(Group group, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Group? oldGroup = await _groupRepository.GetByGroupIdAsync(group.GroupId, cancellationToken);

        if (oldGroup is not null)
            return "Group already exists.";

        await _groupRepository.CreateAsync(group, cancellationToken);

        transaction.Complete();

        return "Group created successfully.";
    }

    public Task<IEnumerable<Group>?> GetAllGroupsAsync(CancellationToken cancellationToken)
    {
        return _groupRepository.GetAllAsync(cancellationToken);
    }
}