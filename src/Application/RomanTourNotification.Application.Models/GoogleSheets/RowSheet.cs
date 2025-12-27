namespace RomanTourNotification.Application.Models.GoogleSheets;

public record RowSheet(
    string LastName,
    string Sum,
    string Manager,
    string Organization,
    bool SentStatementToTourist,
    bool GetStatementFromTourist,
    bool Completed)
{
    public override string ToString()
    {
        return $"""
                Турист: {LastName}, 
                Сумма: {Sum}, 
                ИП: {Organization}
                """;
    }
}