using System.ComponentModel;

namespace RomanTourNotification.Application.Models.Users;

public enum UserRole
{
    [Description("Не указана")]
    Unspecified = 0,

    [Description("Разработчик")]
    Developer = 1,

    [Description("Администратор")]
    Admin = 2,

    [Description("Менеджер")]
    Manager = 3,
}