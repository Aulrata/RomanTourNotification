namespace RomanTourNotification.Application.Models.Gateway;

public enum RequestStatus
{
    InWork = 1,
    Confirmed = 2,
    Refusal = 3,
    Cancelled = 4,
    DocumentsIssued = 5,
    ApplicationClosed = 6,
    New = 7,
    ApplicationInLid = 9,
    PartiallyPaid = 10,
    FullPayment = 11,
}