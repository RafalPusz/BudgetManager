using System;
using System.Text.Json.Serialization;
using BudgetManager.Models;

namespace BudgetManager.Services
{
    internal class BudgetService
    {
        // List of all transactions (both incomes and expenses)
        [JsonInclude]
        public List<Transaction> Transactions { get; private set; } = new();

        // Adds a new transaction to the list
        public void AddTransaction(Transaction transaction) => Transactions.Add(transaction);

        // Removes a transaction from the list
        public void RemoveTransaction(Transaction transaction) => Transactions.Remove(transaction);

        // Returns all income transactions
        public IEnumerable<Income> GetIncomes() => Transactions.OfType<Income>();

        // Returns all expense transactions
        public IEnumerable<Expense> GetExpenses() => Transactions.OfType<Expense>();


        // Calculates total income for a given month and year
        public decimal GetMonthlyIncomeSum(int year, int month)
        {
            return GetIncomes()
                .Where(i => i.Date.Year == year && i.Date.Month == month)
                .Sum(i => i.Amount);
        }

        // Calculates total expenses for a given month and year
        public decimal GetMonthlyExpensesSum(int year, int month)
        {
            return GetExpenses()
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .Sum(e => e.Amount);
        }

        // Calculates the balance for a given month (income - expenses)
        public decimal GetMonthlyBalance(int year, int month) => GetMonthlyIncomeSum(year, month) - GetMonthlyExpensesSum(year,month);
       




        
        
    }
}
