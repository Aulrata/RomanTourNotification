namespace RomanTourNotification.Application.Models.GoogleSheets;

public class TaskDescriptions
{
    public string BaseTask { get; set; } = string.Empty;

    // Branch A: Sum is empty
    public string SentApplicationToTourOperatorTask { get; set; } = string.Empty;

    // Branch B: Sum is not empty — linear pipeline
    public string SentStatementToTouristTask { get; set; } = string.Empty;

    public string GetStatementFromTouristTask { get; set; } = string.Empty;

    public string SentStatementToAccountingTask { get; set; } = string.Empty;

    public string ReceiptPrintedTask { get; set; } = string.Empty;

    public string NoTask { get; set; } = string.Empty;
}
