using RomanTourNotification.Domain.ReturnWorkflow;

namespace RomanTourNotification.Application.Models.GoogleSheets;

public record RowSheet(
    string LastName,
    string Sum,
    string Manager,
    string Organization,
    bool SentStatementToTourist,
    bool GetStatementFromTourist,
    bool SentStatementToAccounting,
    bool ReceiptPrinted,
    bool SentApplicationToTourOperator)
{
    /// <summary>
    /// Gets the pipeline stage this case is at (Branch B: Sum not empty).
    /// The switch covers the strict linear progression; any other combination falls back to PendingSend.
    /// </summary>
    public ReturnStatus Status =>
        (SentStatementToTourist, GetStatementFromTourist, SentStatementToAccounting, ReceiptPrinted) switch
        {
            (false, _, _, _) => ReturnStatus.PendingSend,
            (true, false, _, _) => ReturnStatus.PendingReceive,
            (true, true, false, _) => ReturnStatus.PendingAccounting,
            (true, true, true, false) => ReturnStatus.PendingReceipt,
            (true, true, true, true) => ReturnStatus.Completed,
        };

    public override string ToString()
    {
        return $"""
                Турист: {LastName}, 
                Сумма: {Sum}, 
                ИП: {Organization}
                """;
    }
}
