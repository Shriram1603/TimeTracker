using System;
using System.IO;
using System.Collections.Generic;

public class FileHandler
{
    private readonly string BaseDirectory;

    public FileHandler()
    {
        BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeTrackingApp");
        EnsureDirectoryExists(BaseDirectory);
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public string GetUsersFilePath() => Path.Combine(BaseDirectory, "Users.csv");
    public string GetProjectsFilePath() => Path.Combine(BaseDirectory, "Projects.csv");
    public string GetTimeIndexFilePath() => Path.Combine(BaseDirectory, "TimeIndex.csv");

    public void CreateProjectFolder(string username, string projectName)
    {
        string projectPath = Path.Combine(BaseDirectory, "Users", username, projectName);
        EnsureDirectoryExists(projectPath);
    }

    public void CreateTaskFolder(string username, string projectName, string taskName)
    {
        string taskPath = Path.Combine(BaseDirectory, "Users", username, projectName, taskName);
        EnsureDirectoryExists(taskPath);
    }

    public void CreateSubTaskFolder(string username, string projectName, string taskName, string subTaskName)
    {
        string subTaskPath = Path.Combine(BaseDirectory, "Users", username, projectName, taskName, subTaskName);
        EnsureDirectoryExists(subTaskPath);
    }

    public List<string> GetProjectFolders(string username)
    {
        string userPath = Path.Combine(BaseDirectory, "Users", username);
        return Directory.Exists(userPath) ? new List<string>(Directory.GetDirectories(userPath)) : new List<string>();
    }

    public List<string> GetTaskFolders(string username, string projectName)
    {
        string projectPath = Path.Combine(BaseDirectory, "Users", username, projectName);
        return Directory.Exists(projectPath) ? new List<string>(Directory.GetDirectories(projectPath)) : new List<string>();
    }

    public List<string> GetSubTaskFolders(string username, string projectName, string taskName)
    {
        string taskPath = Path.Combine(BaseDirectory, "Users", username, projectName, taskName);
        return Directory.Exists(taskPath) ? new List<string>(Directory.GetDirectories(taskPath)) : new List<string>();
    }
}
