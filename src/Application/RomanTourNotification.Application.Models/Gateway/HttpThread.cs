namespace RomanTourNotification.Application.Models.Gateway;

public class HttpThread
{
    public int Number { get; init; }

    public int CurrentPage { get; set; }

    public HttpThread(int number, int currentPage)
    {
        Number = number;
        CurrentPage = currentPage;
    }
}