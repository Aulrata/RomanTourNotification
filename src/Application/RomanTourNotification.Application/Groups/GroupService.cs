using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Abstractions.Persistence.Repositories.Groups;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Models.Groups;
using System.Transactions;

namespace RomanTourNotification.Application.Groups;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<GroupService> _logger;

    public GroupService(IGroupRepository groupRepository, ILogger<GroupService> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Group?> AddAsync(Group group, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Group? oldGroup = await _groupRepository.GetByChatIdAsync(group.ChatId, cancellationToken);

        if (oldGroup is not null)
            return null;

        await _groupRepository.CreateAsync(group, cancellationToken);

        transaction.Complete();

        return group;
    }

    public async Task<long> DeleteAsync(long chatId, CancellationToken cancellationToken)
    {
        return await _groupRepository.DeleteByChatIdAsync(chatId, cancellationToken);
    }

    public Task<IEnumerable<Group>> GetAllWorksGroupsAsync(CancellationToken cancellationToken)
    {
        return _groupRepository.GetAllWorksGroupsAsync(cancellationToken);
    }

    public Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _groupRepository.GetAllAsync(cancellationToken);
    }

    public Task<Group?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _groupRepository.GetByChatIdAsync(id, cancellationToken);
    }

    public Task<IEnumerable<GroupType>> GetAllGroupTypesByIdAsync(long groupId, CancellationToken cancellationToken)
    {
        return _groupRepository.GetAllGroupTypesByIdAsync(groupId, cancellationToken);
    }

    public async Task<bool> AddGroupTypeByIdAsync(long groupId, GroupType groupType, CancellationToken cancellationToken)
    {
        IEnumerable<GroupType> types = await _groupRepository.GetAllGroupTypesByIdAsync(groupId, cancellationToken);

        if (types.Contains(groupType))
        {
            _logger.LogInformation("Данный тип у группы уже добавлен");
            return false;
        }

        await _groupRepository.AddGroupTypeByIdAsync(groupId, groupType, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveGroupTypeByIdAsync(long groupId, GroupType groupType, CancellationToken cancellationToken)
    {
        IEnumerable<GroupType> types = await _groupRepository.GetAllGroupTypesByIdAsync(groupId, cancellationToken);

        if (!types.Contains(groupType))
        {
            _logger.LogInformation("Данный тип у группы уже удален");
            return false;
        }

        await _groupRepository.RemoveGroupTypeByIdAsync(groupId, groupType, cancellationToken);
        return true;
    }

    public async Task<bool> AddGroupManager(long groupId, string managerFullname, CancellationToken cancellationToken)
    {
        Group? group = await _groupRepository.GetByChatIdAsync(groupId, cancellationToken);

        if (group?.ManagerFullname == managerFullname)
        {
            _logger.LogInformation("У группы уже добавлен этот менеджер.");
            return false;
        }

        await _groupRepository.AddManagerByIdAsync(groupId, managerFullname, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveGroupManager(long groupId, CancellationToken cancellationToken)
    {
        Group? group = await _groupRepository.GetByChatIdAsync(groupId, cancellationToken);

        if (string.IsNullOrEmpty(group?.ManagerFullname))
        {
            _logger.LogInformation("У группы уже удален менеджер.");
            return false;
        }

        await _groupRepository.RemoveManagerByIdAsync(groupId, cancellationToken);
        return true;
    }
}