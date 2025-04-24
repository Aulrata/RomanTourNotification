using System.ComponentModel;

namespace RomanTourNotification.Application.Models.Gateway;

public enum InformationServiceType
{
    [Description("Отель")]
    Hotel = 1,

    [Description("Трансфер")]
    Transfer = 2,

    [Description("Авиабилет")]
    AirTicket = 6,

    [Description("Страховка")]
    Insurance = 11,

    [Description("Пакетный тур")]
    PackageTour = 12,

    [Description("Услуга по информированию")]
    InformationService = 16154,
}