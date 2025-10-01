using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menedżer_wydatków.Services
{
    internal class UiRenderer
    {
        public void DrawUiFrame(string title, IEnumerable<string> lines, string? footer = null)
        {
            Console.Clear();

            int contentWidth = lines.Any() ? lines.Max(line => line.Length) : 0;
            int titleWidth = title.Length;
            int footerWidth = string.IsNullOrWhiteSpace(footer) ? 0 : footer.Length;

            int finalWidth = Math.Max(Math.Max(contentWidth, titleWidth), footerWidth);

            finalWidth = Math.Clamp(finalWidth, 30, 100);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" {new string('=', finalWidth)} ");
            Console.ResetColor();
            Console.WriteLine($"{title.PadLeft((finalWidth + title.Length) / 2)}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" {new string('=', finalWidth)} ");
            Console.ResetColor();

            foreach (string line in lines)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("|");
                Console.ResetColor();

                Console.Write($"{line.PadRight(finalWidth)}");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("|");
                Console.ResetColor();
            }

            if (!string.IsNullOrWhiteSpace(footer))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"|{new string('=', finalWidth)}|");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("|");
                Console.ResetColor();

                Console.Write($"{footer.PadLeft((finalWidth + footer.Length) / 2)}");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("|");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($" {new string('=', finalWidth)} ");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($" {new string('=', finalWidth)}");
                Console.ResetColor();
            }
        }

        private void ShowMessage(string message, ConsoleColor color = ConsoleColor.Green)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
            Thread.Sleep(1500);
        }

        public void ShowError(string message) => ShowMessage(message, ConsoleColor.Red);
        public void ShowSuccess(string message) => ShowMessage(message, ConsoleColor.Green);
    }
}
