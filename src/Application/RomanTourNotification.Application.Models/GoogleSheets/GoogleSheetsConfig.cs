namespace RomanTourNotification.Application.Models.GoogleSheets;

public class GoogleSheetsConfig
{
    public string Id { get; set; } = string.Empty;

    public string List { get; set; } = string.Empty;

    public string Range { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ColumIndex? ColumIndex { get; set; }

    public string ConfigPath { get; set; } = string.Empty;

    public TaskDescriptions? TaskDescriptions { get; set; }
}