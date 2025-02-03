using RomanTourNotification.Application.Models.Users;

namespace RomanTourNotification.Application.Mappers;

public class UserRoleMapper
{
    public static UserRole? MapRole(string roleName)
    {
        return roleName.ToLower() switch
        {
            "разработчик" => UserRole.Developer,
            "админ" => UserRole.Admin,
            "менеджер" => UserRole.Manager,
            _ => null,
        };
    }
}