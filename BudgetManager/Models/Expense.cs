using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetManager.Models
{
    internal class Expense : Transaction
    {
        private string name = string.Empty;
       
        public string Name
        {
            get => name;
            set
            {
                if(string.IsNullOrWhiteSpace(value) || value.Length < 3)
                {
                    throw new ArgumentException("Nazwa musi być wypełniona!");
                }
                name = value;
            }
        }

        public override string Title => Name;

        public Expense(string name, decimal amount, DateTime date)
        {
            Name = name;
            Amount = amount;
            Date = date;
        }
    }
}
