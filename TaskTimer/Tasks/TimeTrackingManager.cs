using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

public class TimeTrackingManager
{
    private readonly FileHandler _fileHandler;
    private Stopwatch _stopwatch;
    private string _currentFilePath;
    private string _workDescription;
    private bool _isBillable;
    private bool _isRunning;
    private bool _pauseTimerDisplay;

    public TimeTrackingManager(FileHandler fileHandler)
    {
        _fileHandler = fileHandler;
        _stopwatch = new Stopwatch();
    }

    public void StartTimer(string username, string project, string task, string subtask, string workDescription, bool isBillable)
    {
        if (_stopwatch.IsRunning)
        {
            Console.WriteLine("A timer is already running. Stop it first!");
            Thread.Sleep(1500);
            return;
        }

        string subTaskPath = Path.Combine(_fileHandler.GetProjectsFilePath(), "Users", username, project, task, subtask);
        Directory.CreateDirectory(subTaskPath);
        _currentFilePath = Path.Combine(subTaskPath, "TimeEntry.csv");

        _workDescription = workDescription;
        _isBillable = isBillable;
        _stopwatch.Restart();
        _isRunning = true;
        _pauseTimerDisplay = false;

        Console.Clear(); // Clear screen and reserve the first line for the timer
        Console.SetCursorPosition(0, 1);
        Console.WriteLine("Timer started...");

        Thread timerThread = new Thread(DisplayTimer);
        timerThread.Start();
    }

    public void PauseTimerDisplay(bool pause)
    {
        _pauseTimerDisplay = pause;
    }


    private void DisplayTimer()
    {
        while (_isRunning)
        {
            if (_pauseTimerDisplay) 
            {
                Thread.Sleep(500);
                continue;
            }

            Console.SetCursorPosition(0, 0); // Always show timer on the first line
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(new string(' ', Console.WindowWidth)); // Clear the line
            Console.SetCursorPosition(0, 0);
            Console.Write($"⏳ Working on {_workDescription} - Elapsed time: {_stopwatch.Elapsed:hh\\:mm\\:ss}");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        // Clear the timer when stopping
        Console.SetCursorPosition(0, 0);
        Console.Write(new string(' ', Console.WindowWidth));
    }

    public void StopTimer()
    {
        if (!_stopwatch.IsRunning)
        {
            Console.WriteLine("No active timer to stop.");
            return;
        }

        _stopwatch.Stop();
        _isRunning = false;
        TimeSpan duration = _stopwatch.Elapsed;
        string startTime = DateTime.Now.Subtract(duration).ToString("yyyy-MM-dd HH:mm:ss");
        string endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        File.AppendAllText(_currentFilePath, $"{startTime},{endTime},{_workDescription},{(_isBillable ? "Yes" : "No")}\n");

        Console.SetCursorPosition(0, 0);
        Console.Write(new string(' ', Console.WindowWidth)); // Clear timer display
        Console.SetCursorPosition(0, 2); // Move cursor below "Timer stopped" message
        
        Console.WriteLine($"\n✅ Timer stopped. Duration: {duration}");
        Thread.Sleep(1500);
    }

    public string GetUserInput(string prompt)
    {
        _pauseTimerDisplay = true; // Pause timer display to avoid overlap
        Console.SetCursorPosition(0, Console.CursorTop + 1); // Move input below timer
        Console.Write(prompt);
        string input = Console.ReadLine();
        _pauseTimerDisplay = false; // Resume timer display
        return input;
    }
}
