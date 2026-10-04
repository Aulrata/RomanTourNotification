namespace RomanTourNotification.Application.Models.GoogleSheets;

public class ColumIndex
{
    public int LastName { get; set; }

    public int Sum { get; set; }

    public int Manager { get; set; }

    public int Organization { get; set; }

    public int SentApplicationToTourOperator { get; set; }

    public int SentStatementToTourist { get; set; }

    public int GetStatementFromTourist { get; set; }

    public int SentStatementToAccounting { get; set; }

    public int ReceiptPrinted { get; set; }

    public int ReturnedSum { get; set; }

    public int MinimumColumns { get; set; }
}
