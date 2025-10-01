using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BudgetManager.Models
{
    // Abstract base class representing a financial transaction
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Income), "income")]
    [JsonDerivedType(typeof(Expense), "expense")]

    public abstract class Transaction
    {
        // Unique identifier for each transaction
        public Guid Id { get; set; } = Guid.NewGuid();

        // Backing fields for amount and date
        protected decimal amount;
        protected DateTime date;

        // Amount of the transaction with validation
        public decimal Amount
        {
            get => amount;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Kwota wydatku musi być większa od zera!");
                }
                amount = value;
            }
        }

        // Date of the transaction with validation
        public DateTime Date
        {
            get => date;
            set
            {
                if (value.Year < 2019)
                {
                    throw new ArgumentException("Data nie może być z tak dalekiej przeszłości!");
                }

                date = value;
            }
        }

        // Abstract property to be implemented by derived classes (Income or Expense)
        public abstract string Title { get; }

    }
}
