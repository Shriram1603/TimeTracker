namespace TodoList.Todo;

public class Logger
{
    public void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n{message}\n");
        Console.ResetColor();
        Thread.Sleep(1000);
    }

    public void DisplayFailure(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Thread.Sleep(500);
    }

    public void DisplayTitle(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n{message}\n");
        Console.ResetColor();
    }
}