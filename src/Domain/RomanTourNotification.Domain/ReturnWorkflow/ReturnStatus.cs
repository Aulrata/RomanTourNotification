namespace RomanTourNotification.Domain.ReturnWorkflow;

/// <summary>
/// Represents the stage a tourist's return case has reached in the workflow pipeline.
/// Progression: PendingSend → PendingReceive → PendingAccounting → PendingReceipt → Completed.
/// Only rows with a non-empty Sum participate in this pipeline (Branch B).
/// </summary>
public enum ReturnStatus
{
    /// <summary>Statement has not yet been sent to the tourist.</summary>
    PendingSend = 0,

    /// <summary>Statement was sent; waiting for the tourist to return it.</summary>
    PendingReceive = 1,

    /// <summary>Statement received from tourist; need to send to accounting.</summary>
    PendingAccounting = 2,

    /// <summary>Sent to accounting; waiting for receipt to be printed.</summary>
    PendingReceipt = 3,

    /// <summary>All steps completed — excluded from all notification sections.</summary>
    Completed = 4,
}
