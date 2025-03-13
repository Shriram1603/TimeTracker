
namespace TodoList.Model;

/// <summary>
/// This class stores the information of a single
/// user while also generating a unique id for the user.
/// </summary>
public class User
{
    /// <summary>
    /// Holds username of the user.
    /// </summary>
    public string UserName { get; }

    /// <summary>
    /// Holds password of the user.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Indicates if the user is a manager.
    /// </summary>
    public bool IsManager { get; set; }

    /// <summary>
    /// Constructor to create a user.
    /// </summary>
    /// <param name="userName">Unique username as string.</param>
    /// <param name="password">Password as string.</param>
    /// <param name="isManager">Boolean indicating if the user is a manager.</param>
    public User(string userName, string password, bool isManager)
    {
        UserName = userName;
        Password = password;
        IsManager = isManager;
    }

    /// <summary>
    /// Converts a User object to CSV format.
    /// </summary>
    public override string ToString()
    {
        return $"{UserName},{Password},{IsManager}";
    }

    /// <summary>
    /// Creates a User object from a CSV line.
    /// </summary>
    public static User FromCsv(string csvLine)
    {
        var parts = csvLine.Split(',');
        return new User(parts[0], parts[1], bool.Parse(parts[2]));
    }
}
