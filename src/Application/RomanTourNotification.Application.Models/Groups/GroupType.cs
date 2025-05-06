using System.ComponentModel;

namespace RomanTourNotification.Application.Models.Groups;

public enum GroupType
{
    [Description("Не указан")]
    Unspecified = 0,

    [Description("Оплата")]
    Payment = 1,

    [Description("Документы прибытия")]
    Arrival = 2,
}