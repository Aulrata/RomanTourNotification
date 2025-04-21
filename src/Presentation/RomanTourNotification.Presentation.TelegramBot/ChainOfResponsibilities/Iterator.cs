namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public class Iterator
{
    public string CurrentWord { get; private set; }

    public int CountOfCommand { get; private set; }

    private string[] Commands { get; }

    private int CurrentPosition { get; set; } = 0;

    public Iterator(string command)
    {
        Commands = SplitCommand(command);
        CurrentWord = Commands[CurrentPosition];
        CountOfCommand = Commands.Length;
    }

    public void MoveNext()
    {
        CurrentPosition++;

        if (CurrentPosition < Commands.Length)
            CurrentWord = Commands[CurrentPosition];
    }

    private string[] SplitCommand(string command)
    {
        return command.Split(" ");
    }
}