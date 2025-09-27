using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BudgetManager.Models
{
    internal class Income : Transaction
    {
        private string source = string.Empty;

        public string Source
        {
            get => source;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
                {
                    throw new ArgumentException("Źródło musi być znane!");
                }
                source = value;
            }
        }

        public override string Title => Source;

        public Income(string source, decimal amount, DateTime date)
        {
            Source = source;
            Amount = amount;
            Date = date;
        }
    }
}
