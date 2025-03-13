using System;
using TodoList.Model;
using TodoList.Todo;

class Program
{
    static void Main()
    {
        // Initialize dependencies
        FileHandler fileHandler = new();
        UserManager userManager = new(fileHandler);
        Logger logger = new();
        TimeTrackingManager timeTrackingManager = new(fileHandler);
        
        // Pass all dependencies to UserInteractor
        UserInteractor userInteractor = new(userManager, logger, fileHandler, timeTrackingManager);

        RunApplication(userInteractor, logger);
    }

    static void RunApplication(UserInteractor userInteractor, Logger logger)
    {
        while (true)
        {
            try
            {
                Console.Clear();
                string choice = userInteractor.DisplayMenu();
    
                switch (choice)
                {
                    case "Register":
                        userInteractor.Register();
                        break;
                    case "Login":
                        userInteractor.Login();
                        break;
                    case "Exit":
                        ExitApplication();
                        return;
                    default:
                        logger.DisplayFailure("Invalid choice! Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.DisplayFailure($"An unexpected error occurred: {ex.Message}");
            }
        }
    }

    static void ExitApplication()
    {
        Console.Clear();
        Console.WriteLine("Exiting application...");
        Environment.Exit(0);
    }
}
