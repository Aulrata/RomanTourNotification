namespace RomanTourNotification.Application.Models.Groups;

public record Group(
    long Id,
    string Title,
    long ChatId,
    long UserId,
    GroupType GroupType,
    DateTime CreatedAt);