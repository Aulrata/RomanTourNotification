namespace RomanTourNotification.Application.Models.Users;

public class UserState
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public UserRole Role { get; set; } = UserRole.Unspecified;

    public long ChatId { get; set; } = 0;
}