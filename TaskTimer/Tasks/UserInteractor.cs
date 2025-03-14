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
        string userName = PromptForInput("Enter the username: ", "Username cannot be empty!");

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
        string userName = PromptForInput("Enter the username: ", "Username cannot be empty!");
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
    public void ShowDashboard()
    {
        if (_loggedInUser == null)
        {
            _logger.DisplayFailure("No user logged in!");
            return;
        }

        while (true)
        {
            List<string> options = new() { "Create Project" };
            options.AddRange(_fileHandler.GetProjectFolders(_loggedInUser.UserName));

            string choice = CreateDropDown(options, $"Projects - {_loggedInUser.UserName}", "[Up/Down] to navigate, [Enter] to select, [Esc] to exit");

            if (choice == null) return; // Exit on ESC
            if (choice == "Create Project")
            {
                CreateProject(_loggedInUser.UserName);
            }
            else
            {
                ShowTaskMenu(_loggedInUser.UserName, choice);
            }
        }
    }

    private void ShowTaskMenu(string username, string project)
    {
        while (true)
        {
            List<string> options = new() { "Create Task" };
            options.AddRange(_fileHandler.GetTaskFolders(username, project));

            string choice = CreateDropDown(options, $"Tasks in {project}", "[Up/Down] to navigate, [Enter] to select, [Esc] to go back");

            if (choice == null) return; // Back on ESC
            if (choice == "Create Task")
            {
                CreateTask(username, project);
            }
            else
            {
                ShowSubtaskMenu(username, project, choice);
            }
        }
    }

    private void ShowSubtaskMenu(string username, string project, string task)
    {
        while (true)
        {
            List<string> options = new() { "Create Subtask" };
            options.AddRange(_fileHandler.GetSubTaskFolders(username, project, task));

            string choice = CreateDropDown(options, $"Subtasks in {task}", "[Up/Down] to navigate, [Enter] to select, [Esc] to go back");

            if (choice == null) return; // Back on ESC
            if (choice == "Create Subtask")
            {
                CreateSubtask(username, project, task);
            }
            else
            {
                ShowTimerMenu(username, project, task, choice);
            }
        }
    }

    private void ShowTimerMenu(string username, string project, string task, string subtask)
    {
        while (true)
        {
            List<string> options = new() { "Start Timer", "Stop Timer" };

            string choice = CreateDropDown(options, $"Timer for {subtask}", "[Up/Down] to navigate, [Enter] to select, [Esc] to go back");

            if (choice == null) return; // Back on ESC
            if (choice == "Start Timer")
            {
                string workDescription = PromptForInput("Enter work description: ", "Work description cannot be empty!");
                bool isBillable = CreateDropDown(new List<string> { "Yes", "No" }, "Is this work billable?", "[Up/Down] to navigate, [Enter] to select") == "Yes";
                _timeTrackingManager.StartTimer(username, project, task, subtask, workDescription, isBillable);
                _logger.DisplaySuccess("Timer started for subtask!");
            }
            else if (choice == "Stop Timer")
            {
                _timeTrackingManager.StopTimer();
                _logger.DisplaySuccess("Timer stopped!");
            }
        }
    }

    private void CreateProject(string username)
    {
        string projectName = PromptForInput("Enter project name: ", "Project name cannot be empty!");
        _fileHandler.CreateProjectFolder(username, projectName);
        _logger.DisplaySuccess($"Project '{projectName}' created!");
    }

    private void CreateTask(string username, string project)
    {
        string taskName = PromptForInput("Enter task name: ", "Task name cannot be empty!");
        _fileHandler.CreateTaskFolder(username, project, taskName);
        _logger.DisplaySuccess($"Task '{taskName}' created in '{project}'!");
    }

    private void CreateSubtask(string username, string project, string task)
    {
        string subtaskName = PromptForInput("Enter subtask name: ", "Subtask name cannot be empty!");
        _fileHandler.CreateSubTaskFolder(username, project, task, subtaskName);
        _logger.DisplaySuccess($"Subtask '{subtaskName}' created in '{task}'!");
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

    private string PromptForInput(string prompt, string invalidMessage, string warningMessage = "The string size must be within 20!", int maxLength = 20)
{
    int inputRow = Console.CursorTop; // Store the input line position

    _timeTrackingManager.PauseTimerDisplay(true); // Pause timer while taking input

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

        if (input.Length > maxLength)
        {
            Console.SetCursorPosition(0, inputRow + 1);
            _logger.DisplayFailure(warningMessage);
            continue;
        }

        _timeTrackingManager.PauseTimerDisplay(false); // Resume timer after valid input

        return input;
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