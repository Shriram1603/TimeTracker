using System.Globalization;
using TodoList.Model;

namespace TodoList.Todo;


/// <summary>
/// This is the class with which the user
/// interacts to perform operations
/// </summary>
public class UserInteractor
{
  
     private readonly UserManager _userManager;
    private readonly Logger _logger;
    private readonly FileHandler _fileHandler;
    private readonly TimeTrackingManager _timeTrackingManager;
    private User? _loggedInUser; 

    /// <summary>
    /// Constructor to perform dependency injection.
    /// </summary>
    public UserInteractor(UserManager userManager, Logger logger, FileHandler fileHandler, TimeTrackingManager timeTrackingManager)
    {
        _userManager = userManager;
        _logger = logger;
        _fileHandler = fileHandler;
        _timeTrackingManager = timeTrackingManager;
    }


    /// <summary>
    /// Displays the starting menu of the app
    /// while also getting the user choice.
    /// </summary>
    /// <returns>userChoice as integer</returns>
    public string DisplayMenu()
    {

        _logger.DisplayTitle(Constants.Greetings);
        string? UserChoice = CreateDropDown<string>(Constants.Authorization, Constants.Greetings, "");
        return UserChoice;
    }



    /// <summary>
    /// Proptes the user to get info about details to
    /// register the user then send it to <see cref="UserManager"/>
    /// </summary>
    public void Register()
    {
        string userName = PromptForInput<string>("Enter the username: ", "Username cannot be empty!");

        if (_userManager.IsUserPresent(userName))
        {
            Console.Clear();
            _logger.DisplayFailure($"{userName} is already taken! Try another username.");
            return;
        }

        string userPassword = GetUserPassword();

        // Ask if the user is a manager (Dropdown choice)
        string roleChoice = CreateDropDown(new List<string> { "Regular User", "Manager" }, 
                                           "Select User Role:", 
                                           "[Up/Down] to navigate, [Enter] to select");

        bool isManager = roleChoice == "Manager"; // If they chose "Manager", set isManager to true.

        // Create and register the user
        User user = new User(userName, userPassword, isManager);
        _userManager.RegisterUser(user);

        Console.Clear();
        _logger.DisplaySuccess($"User {userName} created successfully as {(isManager ? "Manager" : "Regular User")}!");
    }

    /// <summary>
    /// Prompts the user for login credentials and then sends it to
    /// <see cref="UserManager.IsLoginValid(string, string)"/> to Log the user in.
    /// </summary>
    public void Login()
{
    string userName = PromptForInput<string>("Enter the username: ", "Username cannot be empty!");
    if (_userManager.IsUserPresent(userName))
    {
        string userPassword = GetUserPassword();
        if (_userManager.IsLoginValid(userName, userPassword))
        {
            _loggedInUser = _userManager.GetUser(userName); // Store the User object
            _logger.DisplaySuccess($"Logged in as {_loggedInUser.UserName}!");
            ShowDashboard();
        }
        else
        {
            _logger.DisplayFailure("\nWrong password! Try again.");
        }
    }
    else
    {
        _logger.DisplayFailure($"No user with name {userName}.");
    }
}

    /// <summary>
    /// Displays the dashboard where users can create projects, tasks, subtasks, and start timers.
    /// </summary>
    private void ShowDashboard()
{
    if (_loggedInUser == null)
    {
        _logger.DisplayFailure("No user logged in!");
        return;
    }

    List<string> options = new()
    {
        "Create Project",
        "Create Task",
        "Create Subtask",
        "Start Timer on Subtask",
        "Logout"
    };

    while (true)
    {
        string choice = CreateDropDown(options, $"Dashboard - {_loggedInUser.UserName}", "[Up/Down] to navigate, [Enter] to select");

        switch (choice)
        {
            case "Create Project":
                CreateProject(_loggedInUser.UserName);
                break;
            case "Create Task":
                CreateTask(_loggedInUser.UserName);
                break;
            case "Create Subtask":
                CreateSubtask(_loggedInUser.UserName);
                break;
            case "Start Timer on Subtask":
                StartTimerOnSubtask(_loggedInUser.UserName);
                break;
            case "Logout":
                Console.WriteLine("Logging out...");
                _loggedInUser = null;
                return;
        }
    }
}

    private void CreateProject(string username)
    {
        string projectName = PromptForInput<string>("Enter project name: ", "Project name cannot be empty!");
        _fileHandler.CreateProjectFolder(username, projectName);
        _logger.DisplaySuccess($"Project '{projectName}' created successfully!");
    }

    private void CreateTask(string username)
    {
        string project = SelectProject(username);
        if (project == null) return;

        string taskName = PromptForInput<string>("Enter task name: ", "Task name cannot be empty!");
        _fileHandler.CreateTaskFolder(username, project, taskName);
        _logger.DisplaySuccess($"Task '{taskName}' created in project '{project}'!");
    }

    private void CreateSubtask(string username)
    {
        string project = SelectProject(username);
        if (project == null) return;

        string task = SelectTask(username, project);
        if (task == null) return;

        string subtaskName = PromptForInput<string>("Enter subtask name: ", "Subtask name cannot be empty!");
        _fileHandler.CreateSubTaskFolder(username, project, task, subtaskName);
        _logger.DisplaySuccess($"Subtask '{subtaskName}' created in task '{task}'!");
    }

    private void StartTimerOnSubtask(string username)
    {
        string project = SelectProject(username);
        if (project == null) return;

        string task = SelectTask(username, project);
        if (task == null) return;

        string subtask = SelectSubtask(username, project, task);
        if (subtask == null) return;

        string workDescription = PromptForInput<string>("Enter work description: ", "Work description cannot be empty!");
        bool isBillable = CreateDropDown(new List<string> { "Yes", "No" }, "Is this work billable?", "[Up/Down] to navigate, [Enter] to select") == "Yes";

        _timeTrackingManager.StartTimer(username, project, task, subtask, workDescription, isBillable);
        _logger.DisplaySuccess("Timer started for subtask!");
    }

    private string SelectProject(string username)
    {
        List<string> projects = _fileHandler.GetProjectFolders(username);
        if (projects.Count == 0)
        {
            _logger.DisplayFailure("No projects found! Create a project first.");
            return null;
        }
        return CreateDropDown(projects, "Select a project:", "[Up/Down] to navigate, [Enter] to select");
    }

    private string SelectTask(string username, string project)
    {
        List<string> tasks = _fileHandler.GetTaskFolders(username, project);
        if (tasks.Count == 0)
        {
            _logger.DisplayFailure("No tasks found! Create a task first.");
            return null;
        }
        return CreateDropDown(tasks, "Select a task:", "[Up/Down] to navigate, [Enter] to select");
    }

    private string SelectSubtask(string username, string project, string task)
    {
        List<string> subtasks = _fileHandler.GetSubTaskFolders(username, project, task);
        if (subtasks.Count == 0)
        {
            _logger.DisplayFailure("No subtasks found! Create a subtask first.");
            return null;
        }
        return CreateDropDown(subtasks, "Select a subtask:", "[Up/Down] to navigate, [Enter] to select");
    }


    private T? CreateDropDown<T>(IList<T> items, string message, string menuOptions)
    {
        int selectedIndex = 0;
        ConsoleKey key;
        while (true)
        {
            DrawMenu(items, selectedIndex, message, menuOptions);

            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex - 1 + items.Count) % items.Count;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex + 1 + items.Count) % items.Count;
            }
            else if (key == ConsoleKey.Enter)
            {
                Console.Clear();
                return items[selectedIndex];
            }
            else if (key == ConsoleKey.B || key == ConsoleKey.Escape) // Handle back action
            {
                Console.Clear();
                return default;
            }
        }
    }

    private void DrawMenu<T>(IList<T> categories, int selectedIndex, string message, string menuOptions)
    {
        Console.Clear();
        _logger.DisplayTitle(message);

        // Get the current cursor position after printing the message
        int startRow = Console.CursorTop + 1; // Move below the message

        // Ensure the menu doesn't exceed the window height
        if (startRow + categories.Count >= Console.WindowHeight)
        {
            startRow = Console.WindowHeight - categories.Count - 1;
        }

        // Set cursor to dynamically available top-left position
        Console.SetCursorPosition(0, startRow);

        for (int i = 0; i < categories.Count; i++)
        {
            if (i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"> {categories[i]}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"  {categories[i]}");
            }
        }
        _logger.DisplayTitle(menuOptions);
    }

    private T? PromptForInput<T>(string prompt, string invalidMessage, string? warningMessage = null, T? defaultValue = default)
{
    int inputRow = Console.CursorTop; // Store the input line position

    while (true)
    {
        Console.SetCursorPosition(0, inputRow); // Move cursor back to input line
        Console.Write(new string(' ', Console.WindowWidth)); // Clear input line
        Console.SetCursorPosition(0, inputRow);
        Console.Write(prompt); // Rewrite prompt

        string? input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            Console.SetCursorPosition(0, inputRow + 1);
            _logger.DisplayFailure(invalidMessage);
            continue;
        }

        if (typeof(T) == typeof(string)) return (T)(object)input;
        if (typeof(T) == typeof(int) && int.TryParse(input, out int intValue)) return (T)(object)intValue;
        if (typeof(T) == typeof(double) && double.TryParse(input, out double doubleValue)) return (T)(object)doubleValue;
        if (typeof(T) == typeof(decimal) && decimal.TryParse(input, out decimal decimalValue)) return (T)(object)decimalValue;

        if (typeof(T) == typeof(DateOnly) && DateOnly.TryParseExact(input, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateValue))
        {
            if (warningMessage != null && dateValue < DateOnly.FromDateTime(DateTime.Now))
            {
                Console.SetCursorPosition(0, inputRow + 1);
                _logger.DisplayFailure(warningMessage);
                continue;
            }
            return (T)(object)dateValue;
        }

        Console.SetCursorPosition(0, inputRow + 1);
        _logger.DisplayFailure(invalidMessage);
    }
}



    private string PromptForInputToUpdate(string prompt, string currentValue = "")
    {
        Console.Write(prompt);
        string? input = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(input) ? currentValue : input;
    }

    private string GetUserPassword()
    {
        Console.WriteLine();
        Console.Write("Enter your password: ");
        string password = "";
        ConsoleKeyInfo key;

        while (true)
        {
            key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }

        return password;
    }

}