using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.UI;
using BudgetManager.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetManager.Services
{
    internal class TransactionService
    {
        private readonly BudgetService _budget;
        private readonly InputValidator _validator;
        private readonly UiRenderer _ui;
        private readonly StorageService _storage;

        public TransactionService(BudgetService budget, InputValidator validator, UiRenderer ui, StorageService storage)
        {
            _budget = budget;
            _validator = validator;
            _ui = ui;
            _storage = storage;
        }

        public async Task Add<T>() where T : Transaction
        {
            try
            {
                string title = _validator.ReadNonEmptyString($"Podaj {(typeof(T) == typeof(Income) ? "źródło przychodu" : "nazwę wydatku")}: ");
                decimal amount = _validator.ReadDecimal("Podaj kwotę w PLN: ");
                DateTime date;
                Console.Write("Czy chcesz użyć dzisiejszą datę? (t/n): ");
                while (true)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.T)
                    {
                        date = DateTime.Now;
                        break;
                    }
                    else if (key == ConsoleKey.N)
                    {
                        date = _validator.ReadDate("Podaj datę: ");
                        break;
                    }
                }
                Transaction transaction = typeof(T) == typeof(Income)
                ? new Income(title, amount, date)
                : new Expense(title, amount, date);

                _budget.AddTransaction(transaction);
                await _storage.SaveAsync(_budget);

                _ui.ShowSuccess("Dodano tranzakcję");
            }
            catch (Exception ex)
            {
                _ui.ShowError(ex.Message);
            }
        }
        #region Edit Transaction menu and options
        public async Task Edit<T>() where T : Transaction
        {

            var transactionList = _budget.Transactions.OfType<T>().ToList();

            if (!transactionList.Any())
            {
                _ui.DrawUiFrame($"Lista {(typeof(T) == typeof(Income) ? "przychodów" : "wydatków")}", new List<string> { "Brak danych" });

                _ui.ReturnToMenu();
            }

            var display = transactionList.Select(t => $"{t.Date:d MMMM yyyy} | {t.Title} | {t.Amount} zł").ToList();


            int selection = -1;
            while (true)
            {
                _ui.DrawUiFrame($"Lista {(typeof(T) == typeof(Income) ? "przychodów" : "wydatków")}", display, true ,"Wybierz numer do edycji");
                try
                {
                    selection = _validator.ParseInt(Console.ReadKey(true).KeyChar.ToString());
                }
                catch
                {
                    _ui.ShowError("Niepoprawna opcja");
                    continue;
                }

                if (selection >= 1 && selection <= transactionList.Count)
                {
                    break;
                }
            }

            var transaction = transactionList[selection - 1];

            List<string> ops = ["Nazwa", "Kwota", "Data", "Usuń"];
            _ui.DrawUiFrame($"Edytowany: {transaction.Title}", ops, true, "Wybierz akcję");

            while (true)
            {
                Action action = Console.ReadKey(true).KeyChar switch
                {
                    '1' => () =>
                    {
                        string newTitle = _validator.ReadNonEmptyString($"Podaj {(typeof(T) == typeof(Income) ? "źródło przychodu" : "nazwę wydatku")}: ");
                        if (transaction is Income income)
                        {
                            income.Source = newTitle;
                        }
                        else if (transaction is Expense expense)
                        {
                            expense.Name = newTitle;
                        }
                        _ui.ShowSuccess("Zmieniono nazwę");
                    }
                    ,
                    '2' => () =>
                    {
                        decimal amount = _validator.ReadDecimal("Podaj kwotę w PLN: ");
                        transactionList[selection - 1].Amount = amount;
                        _ui.ShowSuccess("Zmieniono kwotę.");
                    }
                    ,
                    '3' => () =>
                    {
                        DateTime newDate = _validator.ReadDate("Podaj nową datę: ");
                        transactionList[selection - 1].Date = newDate;

                        _ui.ShowSuccess("Zmieniono datę");
                    }
                    ,
                    '4' => () =>
                    {
                        Console.WriteLine($"Czy na pewno chcesz usunąć tranzakcję '{transactionList[selection]}' (T/N);");

                        if (Console.ReadKey(true).KeyChar == 't' || Console.ReadKey(true).KeyChar == 'T')
                        {
                            _budget.RemoveTransaction(transactionList[selection]);
                            _ui.ShowSuccess("Usunięto tranzakcje.");
                        }

                    }
                    ,
                    _ => () => _ui.ShowError("Błędna opcja."!)
                };

                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    _ui.ShowError(ex.Message);
                }
            }

            await _storage.SaveAsync(_budget);
            return;
        }
        #endregion


    }
}
