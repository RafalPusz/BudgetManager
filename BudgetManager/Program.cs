using BudgetManager.Models;
using BudgetManager.Services;
using Menedżer_wydatków.Services;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

// Main entry point of the Budget Manager console application.
// Responsible for managing the program lifecycle, user interface, 
// data persistence and business logic communication.
public class Program
{
    private BudgetService budgetService = new(); // Handles all financial operations (add, edit, calculate balance)
    InputValidator validator = new();
    UiRenderer ui = new();
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
                '5' => () => { ShowIncomesOrExpenses(); return Task.CompletedTask;},
                '6' => () => { ShowIncomesOrExpenses(false); return Task.CompletedTask;},
                '7' => async () =>
                {
                    await storageService.SaveAsync(budgetService);
                    Environment.Exit(0);
                },
                _ => async () => 
                {
                    ui.ShowError("Nieznana opcja");
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
        List<string> menuItems = ["1. Dodaj przychód", "2. Dodaj wydatek", "3. Edytuj przychód", "4. Edytuj wydatek", "5. Pokaż przychody", "6. Pokaż wydatki", "7. Wyjdź"];

        ui.DrawUiFrame("Zarządzanie budżetem", menuItems, $"Bilans na miesiąc {DateTime.Now:MMMM}: {budgetService.GetMonthlyBalance(DateTime.Now.Year, DateTime.Now.Month)} zł"); 
    }

    private void ShowIncomesOrExpenses(bool isIncome = true)  
    {
        List<Transaction> transactionList = isIncome
        ? budgetService.Transactions.OfType<Income>().Cast<Transaction>().ToList()
        : budgetService.Transactions.OfType<Expense>().Cast<Transaction>().ToList();

        var transactionListToStringList = transactionList
            .Select(t => $"{t.Date:d MMMM yyyy} | {t.Title} | {t.Amount} zł")
            .ToList();
            

        if (!transactionListToStringList.Any())
        {
            transactionListToStringList.Add(isIncome ? "Brak przychodów" : "Brak wydatków");
        }

        ui.DrawUiFrame($"Twoje{(isIncome ? "  przychody" : " wydatki")}", transactionListToStringList);
        ReturnToMenu();
    }
    #endregion

    #region Edit Transaction menu and options
    private void EditNameOption(in bool isIncome, List<Transaction> transactionList, in int keyListNumber)
    {   
        try
        {
            string newSourceOrName = validator.ReadNonEmptyString($"Podaj {(isIncome ? "źródło przychodu" : "nazwę wydatku")}: ");
            if (isIncome)
            {
                var income = (Income)transactionList[keyListNumber];
                income.Source = newSourceOrName;
            }
            else
            {
                var expense = (Expense)transactionList[keyListNumber];
                expense.Name = newSourceOrName;
            }
            ui.ShowSuccess("Zmieniono nazwę");
        }
        catch (Exception ex)
        {
            ui.ShowError(ex.Message);
        }
        
    }
    private void EditAmountOptions(List<Transaction> transactionList, in int keyListNumber)
    {
        try
        {
            decimal amount = validator.ReadDecimal("Podaj kwotę w PLN: ");
            transactionList[keyListNumber].Amount = amount;
            ui.ShowSuccess("Zmieniono kwotę.");
        }
        catch (Exception ex)
        {
            ui.ShowError(ex.Message);
        }
    }
    private void EditDateOption(List<Transaction> transactionList, in int keyListNumber)
    {
        try
        {
            DateTime newDate = validator.ReadDate("Podaj nową datę: ");
            transactionList[keyListNumber].Date = newDate;

            ui.ShowSuccess("Zmieniono datę");
        }
        catch (Exception ex)
        {
            ui.ShowError(ex.Message);
        }
    
    }
    private void DeleteTransactionOption(List<Transaction> transactionList, in int keyListNumber)
    {
        Console.WriteLine($"Czy na pewno chcesz usunąć tranzakcję '{transactionList[keyListNumber]}' (T/N);");

        if (Console.ReadKey(true).KeyChar == 't' || Console.ReadKey(true).KeyChar == 'T')
        {
            budgetService.RemoveTransaction(transactionList[keyListNumber]);
            ui.ShowSuccess("Usunięto tranzakcje.");
        }
        
    }
    private async Task EditIncomeOrExpenseData(bool isIncome = true)
    {
        Console.ForegroundColor = ConsoleColor.Green;

        List<Transaction> transactionList = isIncome
        ? budgetService.Transactions.OfType<Income>().Cast<Transaction>().ToList()
        : budgetService.Transactions.OfType<Expense>().Cast<Transaction>().ToList();

        int keyListNumber = 0;

        var transactionListToStringList = transactionList
            .Select(t => $"{t.Date: d MMMM yyyy} | {t.Title} | {t.Amount}")
            .ToList();


        if (!transactionList.Any())
        {
            transactionListToStringList.Add("Brak danych");
        }

        while (keyListNumber <= 0  || keyListNumber > transactionList.Count())
        {
            ui.DrawUiFrame($"Lista {(isIncome ? "przychodów" : "wydatków")}", transactionListToStringList, transactionList.Any() ? "Wybierz z listy pozycję, którą chcesz edytować" : null);

            if(!transactionList.Any())
            {
                ReturnToMenu();
            }

            try
            {
                keyListNumber = validator.ParseInt(Console.ReadKey(true).KeyChar.ToString());
            }
            catch
            {
                ui.ShowError("Nie znaleziono takiej opcji.");
            }
        }

        keyListNumber--;
        List<string> menuItems= ["1. Nazwa", "2. Kwota", "3. Data", "4. Usuń element"];

        ui.DrawUiFrame($"Edytowany element: {transactionList[(int)keyListNumber]}", menuItems, "Wybierz element który chcesz edytować");

        while (true)
        {
            bool endWhile = false;
            Action action = Console.ReadKey(true).KeyChar switch
            {
                '1' => () => {EditNameOption(isIncome, transactionList, in keyListNumber); endWhile = true;},
                '2' => () => {EditAmountOptions(transactionList, in keyListNumber); endWhile = true;},
                '3' => () => {EditDateOption(transactionList, in keyListNumber); endWhile = true; },
                '4' => () => {DeleteTransactionOption(transactionList, in keyListNumber); endWhile = true;}, 
                _ => () => ui.ShowError("Błędna opcja."!)
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
        try
        {
            string sourceOrName = validator.ReadNonEmptyString($"Podaj {(isIncome ? "źródło przychodu" : "nazwę wydatku")}: ");
            decimal amount = validator.ReadDecimal("Podaj kwotę w PLN: ");
            DateTime date;

            while (true)
            {
                Console.Write("Czy chcesz użyć dzisiejszą datę? (t/n): ");
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.T)
                {
                    date = DateTime.Now;
                    break;
                }
                else if(key == ConsoleKey.N)
                {
                    date = validator.ReadDate("Podaj datę: ");
                    break;
                }
            }

            budgetService.AddTransaction(isIncome ? new Income(sourceOrName, amount, date) : new Expense(sourceOrName, amount, date));
            await storageService.SaveAsync(budgetService);
        }
        catch (Exception ex)
        {
            ui.ShowError(ex.Message);
        }
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
