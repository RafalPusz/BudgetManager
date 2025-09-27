using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetManager.Services
{
    internal class Helpers
    {
        // Validates and parses a decimal value from string input
        public decimal DecimalValidation(string input)
        {
            if(decimal.TryParse(
                input.Replace(',', '.'),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException("Błędna wartość!");
            }

        }

        // Validates and parses an integer value from string input
        public int IntiggerValidation(string n)
        {
            if (int.TryParse(n, out int result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException("Błędna wartość!");
            }
        }

        // Validates and parses a DateTime value from string input
        public DateTime DateValidation(string input)
        {
            if (DateTime.TryParse(input, out DateTime result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException("Błędna data! Podaj datę w poprawnym formacie (np. 2025-09-27 lub 27.09.2025)!");
            }
        }


        // Displays an error message in red and pauses briefly
        public void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nBłąd: {message}");
            Console.ResetColor();
            Thread.Sleep(1500);
        }

        // Displays a confirmation message in green and pauses briefly
        public void ShowConfirmMessange(string messange)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(messange);
            Console.ResetColor();
            Thread.Sleep(1500);
        }

        // Reads a non-empty string from console input, repeatedly prompting the user until a valid value is entered
        public string ReadNonEmptyString(string prompt)
        {
            string input = "";
            while (string.IsNullOrWhiteSpace(input))
            {
                Console.Clear();
                Console.Write(prompt);
                input = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(input))
                {
                    ShowError("Błędna/Pusta wartość!");
                }
            }
            return input;
        }

        // Reads a decimal value from console input, repeatedly prompting the user until a valid decimal is entered
        public decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Clear();
                Console.Write(prompt);
                try
                {
                    return DecimalValidation(Console.ReadLine() ?? "");
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        // Reads a DateTime value from console input. 
        // If useToday is true, returns the current date; otherwise repeatedly prompts the user until a valid date is entered
        public DateTime ReadDate(bool useToday = true)
        {
            if (useToday)
            {
                return DateTime.Now;
            }
            while (true)
            {
                Console.Clear();
                Console.Write("\nWpisz datę: ");
                try
                {
                    return DateValidation(Console.ReadLine() ?? "");
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }
    }
}
