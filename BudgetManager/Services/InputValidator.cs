using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menedżer_wydatków.Services
{
    internal class InputValidator
    {
        public string ReadNonEmptyString(string prompt)
        {
            while (true)
            {
                Console.Clear();
                Console.Write(prompt);
                var input = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(input))
                {
                    return input;
                }

                throw new ArgumentException("Pole nie może być puste!");
            }
        }

        public decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Clear();
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (decimal.TryParse(input, out decimal value) && value >= 0)
                {
                    return value;
                }
                throw new ArgumentException("Podaj poprawną liczbę większą lub równą 0!");
            }
        }

        public DateTime ReadDate(string prompt)
        {

            while (true)
            {
                Console.Clear();
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (DateTime.TryParse(input, out DateTime date))
                {
                    return date;
                }

                throw new ArgumentException("Niepoprawny format daty!");
            }
        }

        public int ParseInt(string input)
        {
            if (int.TryParse(input, out int result))
            {
                return result;
            }

            throw new ArgumentException("Podaj poprawną liczbę całkowitą!");
        }

    }   
}
