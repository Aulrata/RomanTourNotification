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
    /// Linear progression: each step requires all previous flags to be true.
    /// </summary>
    public ReturnStatus Status
    {
        get
        {
            if (!SentStatementToTourist)
                return ReturnStatus.PendingSend;

            if (!GetStatementFromTourist)
                return ReturnStatus.PendingReceive;

            if (!SentStatementToAccounting)
                return ReturnStatus.PendingAccounting;

            return ReceiptPrinted ? ReturnStatus.Completed : ReturnStatus.PendingReceipt;
        }
    }

    public override string ToString()
    {
        return $"""
                Турист: {LastName}, 
                Сумма: {Sum}, 
                ИП: {Organization}
                """;
    }
}
