namespace RomanTourNotification.Application.Models.Groups;

public record Group(
    long Id,
    string Title,
    long ChatId,
    long UserId,
    string ManagerFullname,
    GroupType GroupType,
    DateTime CreatedAt);