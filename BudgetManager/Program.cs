using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using BudgetManager.Models;
using BudgetManager.Services;

// Main entry point of the Budget Manager console application.
// Responsible for managing the program lifecycle, user interface, 
// data persistence and business logic communication.
public class Program
{
    private BudgetService budgetService = new(); // Handles all financial operations (add, edit, calculate balance)
    private Helpers helpers = new(); // Utility class for input validation and user messages
    private StorageService storageService = new(); // Manages data loading and saving from/to file

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

            ShowMenu();

            // Map user key input to corresponding actions
            Func<Task> action = Console.ReadKey(true).KeyChar switch
            {
                '1' => () => AddIncomeOrExpenseAsync(),
                '2' => () => AddIncomeOrExpenseAsync(false),
                '3' => () => EditIncomeOrExpenseData(),
                '4' => () => EditIncomeOrExpenseData(false),
                '5' => () => { ShowIncomes(); return Task.CompletedTask;},
                '6' => () => { ShowExpenses(); return Task.CompletedTask;},
                '7' => async () =>
                {
                    await storageService.SaveAsync(budgetService);
                    Environment.Exit(0);
                },
                _ => async () => 
                {
                    helpers.ShowError("Nieznana opcja");
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
        Console.Clear();
        int menuWidth = 40;
        List<string> menuItems = ["1. Dodaj przychód", "2. Dodaj wydatek", "3. Edytuj przychód","4. Edytuj wydatek", "5. Pokaż przychody", "6. Pokaż wydatki", "7. Wyjdź"];
        Console.WriteLine($"{"Zarządzanie budżetem".PadLeft((menuWidth + "Zarządzanie budżetem".Length) / 2)}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($" {new string('=', menuWidth)} ");
        Console.ResetColor();

        foreach (string menuItem in menuItems)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("|");
            Console.ResetColor();
            Console.Write($"{menuItem}{new string(' ', menuWidth - menuItem.Length)}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("|\n");    
            Console.WriteLine($"|{new string(' ', menuWidth)}|");
            Console.ResetColor();
        }

        string bilance = $"Bilans na miesiąc {DateTime.Now:MMMM}";
        string bilanceCount = $"{budgetService.GetMonthlyBalance(DateTime.Now.Year, DateTime.Now.Month)} zł.";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"|{new string('=', menuWidth)}|");
        Console.ResetColor();


        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("|");
        Console.ResetColor();

        Console.Write($"{new string(' ', (menuWidth - bilance.Length) / 2)}{bilance}{new string(' ', (menuWidth - bilance.Length) / 2)}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("|\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("|");
        Console.ResetColor();

        Console.Write($"{new string(' ', (menuWidth - bilanceCount.Length) / 2)}{bilanceCount}{new string(' ', (menuWidth - bilanceCount.Length) / 2)}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("|\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($" {new string('=', menuWidth)} ");
        Console.ResetColor();
       
    }
    private void ShowIncomes()
    {
        Console.Clear();
        if (budgetService.GetIncomes().Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"==========Twoje przychody==========");
            Console.ResetColor();
            foreach (var income in budgetService.GetIncomes())
            {
                Console.WriteLine(income);
            }
        }
        else
        {
            Console.WriteLine("Brak przychodów.");
        }
        ReturnToMenu();
    }
    private void ShowExpenses()
    {
        Console.Clear();
        if (budgetService.GetExpenses().Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"==========Twoje wydatki==========");
            Console.ResetColor();
            foreach (var expense in budgetService.GetExpenses())
            {
                Console.WriteLine(expense);
            }
        }
        else
        {
            Console.WriteLine("Brak wydatków");
        }
        ReturnToMenu();
    }
    #endregion

    #region Edit Transaction menu and options
    private void EditNameOption(in bool isIncome, List<Transaction> transactionList, in int keyListNumber)
    {
        while (true)
        {
            string newSourceOrName = helpers.ReadNonEmptyString($"Podaj {(isIncome ? "źródło przychodu" : "nazwę wydatku")}: ");

            try
            {
                if (isIncome)
                {
                    var income = (Income)transactionList[(int)keyListNumber];
                    income.Source = newSourceOrName;
                }
                else
                {
                    var expense = (Expense)transactionList[(int)keyListNumber];
                    expense.Name = newSourceOrName;
                }
                helpers.ShowConfirmMessange("Zmieniono nazwę");
                break;
            }
            catch (Exception ex)
            {
                helpers.ShowError(ex.Message);
            }
        }
    }
    private void EditAmountOptions(in bool isIncome, List<Transaction> transactionList, in int keyListNumber)
    {
        decimal amount = helpers.ReadDecimal("Podaj kwotę w PLN: ");
        while (true)
        {
            try
            {
                transactionList[(int)keyListNumber].Amount = (decimal)amount;
                helpers.ShowConfirmMessange("Zmieniono kwotę.");
                break;
            }
            catch (Exception ex)
            {
                helpers.ShowError(ex.Message);
            }
        }
    }
    private void EditDateOption(in bool isIncome, List<Transaction> transactionList, in int keyListNumber)
    {
        while (true)
        {
            Console.Clear();
            Console.Write("Podaj nową datę: ");
            try
            {
                DateTime newDate = helpers.DateValidation(Console.ReadLine() ?? "");
                transactionList[(int)keyListNumber].Date = newDate;

                helpers.ShowConfirmMessange("Zmieniono datę");
                break;
            }
            catch (Exception ex)
            {
                helpers.ShowError(ex.Message);
            }
        }
    }
    private void DeleteTransactionOption(in bool isIncome, List<Transaction> transactionList, in int keyListNumber)
    {
        Console.WriteLine($"Czy na pewno chcesz usunąć tranzakcję '{transactionList[(int)keyListNumber]}' (T/N);");
        while (true)
        {
            if (Console.ReadKey(true).KeyChar == 't' || Console.ReadKey(true).KeyChar == 'T')
            {
                budgetService.RemoveTransaction(transactionList[(int)keyListNumber]);
                helpers.ShowConfirmMessange("Usunięto tranzakcje.");
                break;
            }
            else
            {       
                break;
            }
        }
    }
    private async Task EditIncomeOrExpenseData(bool isIncome = true)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"==========Lista {(isIncome ? "przychodów" : "wydatków")}==========");
        Console.ResetColor();

        List<Transaction> transactionList = isIncome
        ? budgetService.Transactions.OfType<Income>().Cast<Transaction>().ToList()
        : budgetService.Transactions.OfType<Expense>().Cast<Transaction>().ToList();

        if (!transactionList.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Brak danych");
            Console.ResetColor();
            return;
        }
        int keyListNumber;
        while (true)
        {
            Console.Clear();
            for (int i = 0; i < transactionList.Count(); i++)
            {
                Console.WriteLine($"{i + 1}. {transactionList[i]}");
            }
            Console.WriteLine("\nWybierz z listy pozycję, którą chcesz edytować.");

            try
            {
                keyListNumber = helpers.IntiggerValidation(Console.ReadKey(true).KeyChar.ToString());

                if (keyListNumber <= transactionList.Count() && keyListNumber != 0)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                helpers.ShowError(ex.Message);
            }
        }

        keyListNumber--;
        Console.Clear();
        Console.WriteLine($"Edytowany element: {transactionList[(int)keyListNumber]}");
        Console.WriteLine("\n\nWybierz element który chcesz edytować:");
        Console.WriteLine("1. Nazwa");
        Console.WriteLine("2. Kwota");
        Console.WriteLine("3. Data");
        Console.WriteLine("4. Usuń element");

        while (true)
        {
            bool endWhile = false;
            Action action = Console.ReadKey(true).KeyChar switch
            {
                '1' => () => {EditNameOption(in isIncome, transactionList, in keyListNumber); endWhile = true;},
                '2' => () => {EditAmountOptions(in isIncome, transactionList, in keyListNumber); endWhile = true;},
                '3' => () => {EditDateOption(in isIncome, transactionList, in keyListNumber); endWhile = true; },
                '4' => () => {DeleteTransactionOption(in isIncome, transactionList, in keyListNumber); endWhile = true;}, 
                _ => () => helpers.ShowError("Błędna opcja."!)
            };
            action();
            if(endWhile)
            {
                break;
            }
        }

        await storageService.SaveAsync(budgetService);
        return;

    }
    #endregion

    private async Task AddIncomeOrExpenseAsync(bool isIncome = true)
    {
        var (sourceOrName, amount, date) = GetIncomeOrExpenseData(isIncome);
        try
        {
            budgetService.AddTransaction(isIncome ? new Income(sourceOrName, amount, date) : new Expense(sourceOrName, amount, date));
            await storageService.SaveAsync(budgetService);
        }
        catch (Exception ex)
        {
            helpers.ShowError(ex.Message);
        }
        await Task.Delay(300);
    }
    private (string sourceOrName, decimal amount, DateTime date) GetIncomeOrExpenseData(bool isIncome = true)
    {

        string sourceOrName = helpers.ReadNonEmptyString($"Podaj {(isIncome ? "źródło przychodu" : "nazwę wydatku")}: ");
        decimal amount = helpers.ReadDecimal("Podaj kwotę w PLN: ");
  
        DateTime date;
        while (true)
        {
            Console.Write("Czy chcesz użyć dzisiejszą datę? (t/n): ");
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.T || key == ConsoleKey.N)
            {
                date = helpers.ReadDate(key == ConsoleKey.T);
                break;
            }
            else
            {
                Console.WriteLine("Niepoprawny klawisz!");
            }
        }

        return (sourceOrName, amount, date);
    }
    private void ReturnToMenu()
    {
        Console.WriteLine("\nKliknij 'p', aby powrócić do menu.");
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
        }
        while (true)
        {
            if (Console.ReadKey(true).Key == ConsoleKey.P)
            {
                return;
            }
        }
    }
}
