using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.UI;
using BudgetManager.Validators;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

// Main entry point of the Budget Manager console application.
// Responsible for managing the program lifecycle, user interface, 
// data persistence and business logic communication.
public class Program
{
    private BudgetService budgetService; // Handles all financial operations (add, edit, calculate balance)
    private readonly InputValidator inputValidator;
    private readonly UiRenderer uiRenderer;
    private readonly StorageService storageService; // Manages data loading and saving from/to file
    private TransactionMenu? transactionMenu;
    private TransactionService? transactionService;

    public Program()
    {
        budgetService = new();
        inputValidator = new();
        uiRenderer = new();
        storageService = new();
       
    }

    #region Engine this application
    // Application entry point. Initializes and starts the program asynchronously.
    public static async Task Main(string[] args)
    {
        var program = new Program();
        await program.RunAsync(args);
    }

    // Initializes services, loads budget data, 
    // and starts the main application menu loop.
    private async Task RunAsync(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("========== Zarządzanie budżetem ==========");
        Console.ResetColor();

        // Load budget data from file with loading animation
        budgetService = await ShowLoadingAndLoadBudgetAsync();
        transactionMenu = new(budgetService, uiRenderer);
        transactionService = new(budgetService, inputValidator, uiRenderer, storageService);

        //Automatically save data when the program exits
        AppDomain.CurrentDomain.ProcessExit += async (_, __) =>
        {
            await storageService.SaveAsync(budgetService);
        };
       
        await RunMainMenuAsync();
    }


    // Displays a simple loading animation while asynchronously loading budget data.
    private async Task<BudgetService> ShowLoadingAndLoadBudgetAsync()
    {
        BudgetService? tempBudget = null;

        // Display loading animation in parallel
        var loadingTask = Task.Run(async () =>
        {
            Console.Write("Ładowanie");
            int counter = 0;
            while (tempBudget == null)
            {
                Console.Write(".");
                await Task.Delay(500);
                counter++;
                if (counter == 3)
                {
                    Console.Write("\b\b\b   \b\b\b");
                    counter = 0;
                }
            }
            Console.WriteLine();
        });

        // Load data asynchronously
        tempBudget = await storageService.LoadAsync();
        await loadingTask;

        return tempBudget;
    }

    // Displays the main menu and handles user input.
    // Each option triggers a specific program action.
    private async Task RunMainMenuAsync()
    {
        while (true)
        {

            if (transactionService == null || transactionMenu == null)
            {
                uiRenderer.ShowError("Błąd inicjalizacji — spróbuj ponownie uruchomić program.");
                return;
            }
            ShowMenu();

            // Map user key input to corresponding actions
            Func<Task> action = Console.ReadKey(true).KeyChar switch
            {
                '1' => () => transactionService.Add<Income>(),
                '2' => () => transactionService.Add<Expense>(),
                '3' => () => transactionService.Edit<Income>(),
                '4' => () => transactionService.Edit<Expense>(),
                '5' => () => { transactionMenu.Show<Income>(); return Task.CompletedTask;},
                '6' => () => { transactionMenu.Show<Expense>(); return Task.CompletedTask;},
                '7' => async () =>
                {
                    await storageService.SaveAsync(budgetService);
                    Environment.Exit(0);
                },
                _ => async () => 
                {
                    uiRenderer.ShowError("Nieznana opcja");
                    await Task.Delay(200);
                },
            };

            await action();
        }
    }
    #endregion

    #region Methods that display data
    private void ShowMenu()
    {
        List<string> menuItems = ["Dodaj przychód", "Dodaj wydatek", "Edytuj przychód", "Edytuj wydatek", "Pokaż przychody", "Pokaż wydatki", "Wyjdź"];

        uiRenderer.DrawUiFrame("Zarządzanie budżetem", menuItems, true, $"Bilans na miesiąc {DateTime.Now:MMMM}: {budgetService.GetMonthlyBalance(DateTime.Now.Year, DateTime.Now.Month)} zł"); 
    }

    #endregion

}
