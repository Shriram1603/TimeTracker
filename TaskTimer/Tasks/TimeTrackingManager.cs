using System;
using System.IO;
using System.Diagnostics;

public class TimeTrackingManager
{
    private readonly FileHandler _fileHandler;
    private Stopwatch _stopwatch;
    private string _currentFilePath;
    private string _workDescription;
    private bool _isBillable;

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
            return;
        }

        string subTaskPath = Path.Combine(_fileHandler.GetProjectsFilePath(), "Users", username, project, task, subtask);
        Directory.CreateDirectory(subTaskPath);
        _currentFilePath = Path.Combine(subTaskPath, "TimeEntry.csv");

        _workDescription = workDescription;
        _isBillable = isBillable;
        _stopwatch.Restart();
        Console.WriteLine("Timer started...");
    }

    public void StopTimer()
    {
        if (!_stopwatch.IsRunning)
        {
            Console.WriteLine("No active timer to stop.");
            return;
        }

        _stopwatch.Stop();
        TimeSpan duration = _stopwatch.Elapsed;
        string startTime = DateTime.Now.Subtract(duration).ToString("yyyy-MM-dd HH:mm");
        string endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        File.AppendAllText(_currentFilePath, $"{startTime},{endTime},{_workDescription},{(_isBillable ? "Yes" : "No")}\n");
        Console.WriteLine($"Timer stopped. Duration: {duration}");
    }
}

