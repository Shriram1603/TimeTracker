using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TodoList.Model;

public class UserManager
{
    private IList<User> userList;
    private readonly FileHandler _fileHandler;

    public UserManager(FileHandler fileHandler)
    {
        _fileHandler = fileHandler;
        userList = LoadUsers();
    }

    /// <summary>
    /// Registers a new user if they do not already exist.
    /// </summary>
    public bool RegisterUser(User user)
    {
        if (IsUserPresent(user.UserName))
        {
            Console.WriteLine("User already exists.");
            return false;
        }

        userList.Add(user);
        SaveUsers();
        return true;
    }

    /// <summary>
    /// Saves the user list to the CSV file.
    /// </summary>
    private void SaveUsers()
    {
        var usersFilePath = _fileHandler.GetUsersFilePath();
        List<string> lines = userList.Select(u => $"{u.UserName},{u.Password},{u.IsManager}").ToList();
        File.WriteAllLines(usersFilePath, lines);
    }

    /// <summary>
    /// Loads users from the CSV file.
    /// </summary>
    private List<User> LoadUsers()
    {
        var usersFilePath = _fileHandler.GetUsersFilePath();
        if (!File.Exists(usersFilePath)) return new List<User>();

        var lines = File.ReadAllLines(usersFilePath);
        return lines.Select(line =>
        {
            var parts = line.Split(',');
            bool isManager = parts.Length > 2 && bool.TryParse(parts[2], out bool result) ? result : false;
            return new User(parts[0], parts[1], isManager);
        }).ToList();
    }

    /// <summary>
    /// Checks if login credentials are valid.
    /// </summary>
    public bool IsLoginValid(string userName, string password)
    {
        var user = userList.FirstOrDefault(u => u.UserName == userName);
        return user != null && user.Password == password;
    }

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    public User? GetUser(string userName)
    {
        return userList.FirstOrDefault(u => u.UserName == userName);
    }

    /// <summary>
    /// Checks if a user exists.
    /// </summary>
    public bool IsUserPresent(string userName)
    {
        return userList.Any(u => u.UserName == userName);
    }
}
