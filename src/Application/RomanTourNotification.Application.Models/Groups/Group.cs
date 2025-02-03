namespace RomanTourNotification.Application.Models.Groups;

public record Group(
    long? Id,
    string Title,
    long GroupId,
    long UserId,
    bool Approve,
    GroupType GroupType,
    DateTime CreatedAt);