namespace RomanTourNotification.Application.Models.Users;

public record User(
    long Id,
    string FirstName,
    string? LastName,
    UserRole Role,
    long ChatId,
    DateTime CreatedAt);
