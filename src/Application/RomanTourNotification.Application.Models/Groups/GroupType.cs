using System.ComponentModel;

namespace RomanTourNotification.Application.Models.Groups;

public enum GroupType
{
    [Description("Не указан")]
    Unspecified = 0,

    [Description("Оплата")]
    Payment = 1,

    [Description("Возвраты")]
    Return = 3,

    [Description("Чеки")]
    Receipt = 4,

    [Description("Документы на вылет")]
    DocumentsForDeparture = 5,

    [Description("Авиабилеты")]
    AirTickets = 6,
}