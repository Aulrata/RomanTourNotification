using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Users;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public record HandlerServices(
    IUserService UserService,
    IGroupService GroupService,
    ILoadEmployees LoadEmployees);