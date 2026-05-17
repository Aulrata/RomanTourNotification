using System.ComponentModel;

namespace RomanTourNotification.Application.Models.Gateway;

public enum PaymentType
{
    [Description("Расчеты с клиентом")]
    Client = 1,

    [Description("Расчеты с партнером")]
    Partner = 2,
}