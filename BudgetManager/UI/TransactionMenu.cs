using BudgetManager.Models;
using BudgetManager.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetManager.UI
{
    internal class TransactionMenu
    {
        private readonly BudgetService _budget;
        private readonly UiRenderer _ui;

        public TransactionMenu(BudgetService budget, UiRenderer ui)
        {
            _budget = budget;
            _ui = ui;
        }
        public void Show<T>() where T : Transaction
        {
            var list = _budget.Transactions.OfType<T>().ToList();
            var display = list.Select(t => $"{t.Date:d MMMM yyyy} | {t.Title} | {t.Amount} zł").ToList();

            if (!display.Any())
            {
                display.Add(typeof(T) == typeof(Income) ? "Brak przychodów" : "Brak wydatków");
            }

            _ui.DrawUiFrame($"Twoje {(typeof(T) == typeof(Income) ? "przychody" : "wydatki")}", display);

            _ui.ReturnToMenu();
        }
    }
}
