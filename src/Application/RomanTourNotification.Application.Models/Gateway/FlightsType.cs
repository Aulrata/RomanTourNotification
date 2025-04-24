using System.ComponentModel;

namespace RomanTourNotification.Application.Models.Gateway;

public enum FlightsType
{
    [Description("Не указан")]
    Unspecified = 0,

    [Description("Регулярный")]
    Regular = 1,

    [Description("Чартерный")]
    Charter = 2,
}